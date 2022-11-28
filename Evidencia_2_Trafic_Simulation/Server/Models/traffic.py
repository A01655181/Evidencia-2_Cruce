import mesa
import numpy as np
import random
import math

def move(agent):

    # Get head position
    curr_x, curr_y = agent.pos
    curr_pos = [curr_x, curr_y]

    will_ignore = False

    if isinstance(agent, Ambulance):
        pass
    else:
        if agent.crazy_timer <= 0: # we are out of patience
            if agent.status == 1:
                agent.status = 2
                agent.crazy_timer = random.randrange(30, 50)

            elif agent.status == 2:
                will_ignore = True

    check_crash = agent.model.grid.get_cell_list_contents(agent.pos)
    del_arr = []

    if len(check_crash) > 2: # there are more than one agent in the same cell
            # Another object in the same cell

        for obj in check_crash:
            if isinstance(obj, Car) or isinstance(obj, Ambulance) or isinstance(obj, AmbulanceTail):
                del_arr.append(obj)

        if len(del_arr) > 1:
            agent.model.crashes_num += 1

            for agent_to_del in del_arr:
                if isinstance(agent_to_del, Ambulance) or isinstance(agent_to_del, Car):
                    agent.model.vh_scheduler.remove(agent_to_del)
                    agent.model.grid.remove_agent(agent_to_del)
                    agent.model.curr_cars -= 1
                else:
                     agent.model.grid.remove_agent(agent_to_del.head)
                     agent.model.vh_scheduler.remove(agent_to_del.head)
                     agent.model.grid.remove_agent(agent_to_del)
                     agent.model.curr_cars -= 1
                return


    # 0 is normal, 1 is pressured, 2 is desesperated, 3 is ambulance
    if agent.status == 0:

        possible_steps = agent.model.grid.get_neighborhood(
            agent.pos, moore=True, include_center=True
        )

        opts = []
        for steps in possible_steps:
            cannot_use_step = False
            searching = agent.model.grid.get_cell_list_contents([steps])

            if searching == []:
                opts.append(steps)
            elif searching[0].unique_id == agent.unique_id:
                opts.append(steps)
            else:
                for obj in searching:
                    if isinstance(obj, TrafficLight) and obj.status != 1:
                        cannot_use_step = False
                        break
                    else:
                        cannot_use_step = True
                        break

                if cannot_use_step is True:
                    continue
                else:
                    opts.append(steps)


        # After adding all possible cells filtered agent-wise, check if cells are in either the current street as agent or in the intersection array
        # But first, check if any opts left, other wise only return and set speed of the agent to 0
        if len(opts) == 0:
            agent.velocity = 0
            return

        in_inter = False
        in_street = False
        final_opts = []
        for coords in opts:
            x_next, y_next = coords
            next_loc = [x_next, y_next]
            for location in agent.model.streets[agent.curr_street][agent.curr_side]:
                if location == next_loc:
                    in_street = True
                    final_opts.append(next_loc)
                    break
            # Or if in intersection
            for inter in agent.model.intersection:
                if next_loc == inter:
                    in_iter = True
                    final_opts.append(next_loc)
                    break
            for location in agent.model.streets[agent.final_street][agent.final_side]:
                if location == next_loc:
                    in_street = True
                    final_opts.append(next_loc)
                    break
        min_val = 100000
        best = []
        for locations in final_opts:
            x, y = locations
            new_point = [x, y]
            aux = get_distance(new_point, agent.final_des)
            if aux < min_val:
                best = new_point
                min_val = aux
        if (len(best) > 1):
            agent.model.grid.move_agent(agent, tuple(e for e in best))

        if best == curr_pos: # if did not move
            agent.waiting_time += 1

    elif agent.status == 1:

        for i in range(agent.velocity):

            possible_steps = agent.model.grid.get_neighborhood(
                agent.pos, moore=True, include_center=True
            )

            opts = []
            for steps in possible_steps:
                cannot_use_step = False
                searching = agent.model.grid.get_cell_list_contents([steps])

                if searching == []:
                    opts.append(steps)
                elif searching[0].unique_id == agent.unique_id:
                    opts.append(steps)
                else:
                    for obj in searching:

                        if isinstance(obj, TrafficLight) and obj.status != 1:
                            cannot_use_step = False
                            break
                        elif isinstance(obj, TrafficLight) and obj.status == 1:
                            agent.crazy_timer -= 1
                            cannot_use_step = True
                            break
                        else:
                            cannot_use_step = True
                            break

                    if cannot_use_step is True:
                        continue
                    else:
                        opts.append(steps)

            # After adding all possible cells filtered agent-wise, check if cells are in either the current street as agent or in the intersection array
            # But first, check if any opts left, other wise only return and set speed of the agent to 0
            if len(opts) == 0:
                agent.velocity = 0
                return

            in_inter = False
            in_street = False
            final_opts = []
            for coords in opts:
                x_next, y_next = coords
                next_loc = [x_next, y_next]
                for location in agent.model.streets[agent.curr_street][agent.curr_side]:
                    if location == next_loc:
                        in_street = True
                        final_opts.append(next_loc)
                        break
                # Or if in intersection
                for inter in agent.model.intersection:
                    if next_loc == inter:
                        in_iter = True
                        final_opts.append(next_loc)
                        break
                for location in agent.model.streets[agent.final_street][agent.final_side]:
                    if location == next_loc:
                        in_street = True
                        final_opts.append(next_loc)
                        break
            min_val = 100000
            best = []
            for locations in final_opts:
                x, y = locations
                new_point = [x, y]
                aux = get_distance(new_point, agent.final_des)
                if aux < min_val:
                    best = new_point
                    min_val = aux
            if (len(best) > 1):
                agent.model.grid.move_agent(agent, tuple(e for e in best))

            if best == curr_pos: # if did not move
                agent.waiting_time += 1

    # STATUS 2
    elif agent.status == 2:

        for i in range(agent.velocity):

            possible_steps = agent.model.grid.get_neighborhood(
                agent.pos, moore=True, include_center=True
            )

            opts = []
            for steps in possible_steps:
                cannot_use_step = False
                searching = agent.model.grid.get_cell_list_contents([steps])

                if searching == []:
                    opts.append(steps)
                elif searching[0].unique_id == agent.unique_id:
                    opts.append(steps)
                else:
                    for obj in searching:
                        if isinstance(obj, TrafficLight) and obj.status != 1:
                            cannot_use_step = False
                            break
                        elif isinstance(obj, TrafficLight) and obj.status == 1:
                            if will_ignore is True:
                                cannot_use_step = False
                                break
                            else:
                                agent.crazy_timer -= 1
                                agent.waiting_time += 1
                                cannot_use_step = True
                                break
                        else:
                            cannot_use_step = True
                            break

                    if cannot_use_step is True:
                        continue
                    else:
                        opts.append(steps)

            # After adding all possible cells filtered agent-wise, check if cells are in either the current street as agent or in the intersection array
            # But first, check if any opts left, other wise only return and set speed of the agent to 0
            if len(opts) == 0:
                agent.velocity = 0
                return

            in_inter = False
            in_street = False
            final_opts = []

            for coords in opts:
                x_next, y_next = coords
                next_loc = [x_next, y_next]

                for location in agent.model.streets[agent.curr_street][agent.curr_side]:
                    if location == next_loc:
                        in_street = True
                        final_opts.append(next_loc)
                        break

                # Or if in intersection
                for inter in agent.model.intersection:
                    if next_loc == inter:
                        in_iter = True
                        final_opts.append(next_loc)
                        break

                for location in agent.model.streets[agent.final_street][agent.final_side]:
                    if location == next_loc:
                        in_street = True
                        final_opts.append(next_loc)
                        break

            min_val = 100000
            best = []
            for locations in final_opts:
                x, y = locations
                new_point = [x, y]
                aux = get_distance(new_point, agent.final_des)
                if aux < min_val:
                    best = new_point
                    min_val = aux
            if (len(best) > 1):
                agent.model.grid.move_agent(agent, tuple(e for e in best))

            if best == curr_pos: # if did not move
                agent.waiting_time += 1

    # MOVING AMBULANCE
    elif agent.status == 3: # is ambulance

        for i in range(4):

            # Move ambulance tail to next position
            possible_steps = agent.model.grid.get_neighborhood(
                agent.pos, moore=True, include_center=True
            )

            opts = []
            for steps in possible_steps:
                cannot_use_step = False
                searching = agent.model.grid.get_cell_list_contents([steps])

                if searching == []:
                    opts.append(steps)
                elif searching[0].unique_id == agent.unique_id:
                    opts.append(steps)
                else:
                    for obj in searching:

                        if isinstance(obj, Sidewalk) or isinstance(obj, Car) or isinstance(obj, Ambulance) or isinstance(obj, AmbulanceTail):
                            cannot_use_step = True
                            break # cannot use this stuff

                        elif isinstance(obj, TrafficLight):
                            cannot_use_step = False
                            break

                    if cannot_use_step is True:
                        continue
                    else:
                        opts.append(steps)

            # After adding all possible cells filtered agent-wise, check if cells are in either the current street as agent or in the intersection array
            # But first, check if any opts left, other wise only return and set speed of the agent to 0
            if len(opts) == 0:
                agent.velocity = 0
                return

            in_inter = False
            in_street = False
            final_opts = []
            for coords in opts:
                x_next, y_next = coords
                next_loc = [x_next, y_next]
                for location in agent.model.streets[agent.curr_street][agent.curr_side]:
                    if location == next_loc:
                        in_street = True
                        final_opts.append(next_loc)
                        break

                # Or if in intersection
                for inter in agent.model.intersection:
                    if next_loc == inter:
                        in_iter = True
                        final_opts.append(next_loc)
                        break

                for location in agent.model.streets[agent.final_street][agent.final_side]:
                    if location == next_loc:
                        in_street = True
                        final_opts.append(next_loc)
                        break

            min_val = 100000
            best = []
            for locations in final_opts:
                x, y = locations
                new_point = [x, y]
                aux = get_distance(new_point, agent.final_des)
                if aux < min_val:
                    best = new_point
                    min_val = aux
            if (len(best) > 1):
                agent.model.grid.move_agent(agent, tuple(e for e in best))

            if best != curr_pos: # if did move
                # Move ambulance tail to head
                tail_inst = agent.tail # instance of tail
                agent.model.grid.move_agent(tail_inst, tuple(e for e in curr_pos))
            else: # did not move
                agent.waiting_time += 1


# Some useful methods that are not dependent of an agent
def get_distance(p, q):
    """ Returns euclidean distance from A to B"""
    return abs(q[0] - p[0]) + abs(q[1] - p[1])
    # return math.sqrt((q[1] - p[1])**2 + (q[0] - p[0])**2)


class Sidewalk(mesa.Agent):
    """An agent that sims the sidewalk of the street"""

    def __init__(self, unique_id, model):
        super().__init__(unique_id, model)


class DebugAgents(mesa.Agent):
    def __init__(self, unique_id, status, model):
        super().__init__(unique_id, model)
        self.status = status


class TrafficLight(mesa.Agent):
    """ Traffic light agent """

    def __init__(self, unique_id, direction, model):
        super().__init__(unique_id, model)
        self.status = 0
        self.direction = direction

    def step(self):
        if self.model.yellow_light is True:
            if self.status == 3:
                self.status = 2
                return

        if self.model.prio == []:
            self.status = 0
            return
        else:
            if self.direction == self.model.prio[0]:
                self.status = 3
            else:
                self.status = 1


class Ambulance(mesa.Agent): # head
    """An ambulance agent"""
    def __init__(self, unique_id, tail, model):
        super().__init__(unique_id, model)
        self.status = 3 # 0 is normal, 1 is pressured, 2 is desesperated, 3 is ambulance
        self.velocity = 4 # it will travel only a meter at the time

        # Reference location with name
        self.curr_side = "" # initial side of the street
        self.curr_street = "" # side street

        self.final_street = ""
        self.final_side = ""

        self.final_des = []
        self.tail = tail
        self.waiting_time = 0


    def step(self):
        # Check for self if another object is in the sane cell
        des_x, des_y = self.pos
        curr_pos = [des_x, des_y]

        # If curr pos is inside any dispawn
        for dispawn in self.model.dispawn:
            if curr_pos == dispawn:
                self.model.grid.remove_agent(self.tail)
                self.model.vh_scheduler.remove(self)
                self.model.grid.remove_agent(self)
                self.model.curr_cars -= 1

                self.model.succesfull += 1
                return

        move(self)


class AmbulanceTail(mesa.Agent):
    def __init__(self, unique_id, model):
            super().__init__(unique_id, model)
            self.status = 3 # 0 is normal, 1 is pressured, 2 is desesperated, 3 is ambulance
            self.head = None


class Car(mesa.Agent):
    """An agent that sims a roomba"""
    def __init__(self, unique_id, model):
        super().__init__(unique_id, model)
        self.status = 0 # 0 is normal, 1 is pressured, 2 is desesperated
        self.velocity = 0 # it will travel only a meter at the time

        # Reference location with name
        self.curr_side = "" # initial side of the street
        self.curr_street = "" # side street

        self.final_street = ""
        self.final_side = ""

        self.final_des = []

        self.crazy_timer = 0
        self.waiting_time = 0

    def step(self):
        des_x, des_y = self.pos
        curr_pos = [des_x, des_y]

        # If curr pos is inside any dispawn
        for dispawn in self.model.dispawn:
            if curr_pos == dispawn:
                self.model.vh_scheduler.remove(self)
                self.model.grid.remove_agent(self)
                self.model.curr_cars -= 1
                self.model.succesfull += 1
                return

        move(self)


# INTERSECTION GO HERE
class IntersectionModel(mesa.Model):

    def __init__(self, max_cars_num, debug=False):
        self.max_cars = max_cars_num
        self.debug = debug
        self.curr_cars = 0 # initially zero

        self.kill_agents = [] # agents to kill after each step

        self.grid = mesa.space.MultiGrid(50, 50, False) # create the space of a width and height room_width, room_height and no torodoidal

        # Creating different schedulers
        self.tl_scheduler = mesa.time.RandomActivation(self) # scheduler for steps
        self.vh_scheduler = mesa.time.RandomActivation(self) # scheduler for steps
        self.running = True # running while this is true

        self.prio = []
        self.time = 0

        self.tf_cycle = False
        self.vel_cycle = False
        self.yellow_light = False

        # Vars for data
        self.crashes_num = 0
        self.succesfull = 0

        # Sidewalks:
        x_val = np.union1d(np.array([i for i in range(18)]), np.array([i for i in range(24, 50)]))
        y_val = np.union1d(np.array([i for i in range(22)]), np.array([i for i in range(28, 50)]))

        self.unique_ids = 0

        # Creating the map
        for x in x_val:
            for y in y_val:
                agent = Sidewalk(self.unique_ids, self) # constructor for Sidewalk
                self.grid.place_agent(agent, (x, y))
                self.unique_ids += 1

        # Creating the traffic lights agents
        tl_one_x = [17]
        tl_one_y = [25, 26, 27]

        for x in tl_one_x:
            for y in tl_one_y:
                agent = TrafficLight(self.unique_ids, "left", self) # constructor for Sidewalk
                self.tl_scheduler.add(agent) # adds agent to scheduler
                self.grid.place_agent(agent, (x, y))
                self.unique_ids += 1

        tl_two_x = [24]
        tl_two_y = [22, 23, 24]

        for x in tl_two_x:
            for y in tl_two_y:
                agent = TrafficLight(self.unique_ids, "right", self) # constructor for Sidewalk
                self.tl_scheduler.add(agent) # adds agent to scheduler
                self.grid.place_agent(agent, (x, y))
                self.unique_ids += 1

        tl_three_x = [18, 19, 20]
        tl_three_y = [21]

        for x in tl_three_x:
            for y in tl_three_y:
                agent = TrafficLight(self.unique_ids, "down", self) # constructor for Sidewalk
                self.tl_scheduler.add(agent) # adds agent to scheduler
                self.grid.place_agent(agent, (x, y))
                self.unique_ids += 1

        tl_four_x = [21, 22, 23]
        tl_four_y = [28]

        for x in tl_four_x:
            for y in tl_four_y:
                agent = TrafficLight(self.unique_ids, "up", self) # constructor for Sidewalk
                self.tl_scheduler.add(agent) # adds agent to scheduler
                self.grid.place_agent(agent, (x, y))
                self.unique_ids += 1

        """ CREATING STATIC AGENTS FOR TESTING BEHAVIOR OR DEBUGGING"""
        self.spawn = [
                    [21, 49], # up-one
                    [22, 49], # up-one
                    [23, 49], # up-one

                    [18, 0], # down
                    [19, 0], # down
                    [20, 0], # down

                    [0, 25], # left
                    [0, 26], # left
                    [0, 27], # left

                    [49, 22], # right
                    [49, 23], # right
                    [49, 24], # right
                ]

        self.dispawn = [
                    [18, 49], # up
                    [19, 49], # up
                    [20, 49], # up

                    [21, 0], # down
                    [22, 0], # down
                    [23, 0], # down

                    [0,22], # left
                    [0,23], # left
                    [0,24], # left

                    [49, 25], # right
                    [49, 26], # right
                    [49, 27], # right
                ]

        # Intersections
        self.intersection = []

        x_val_int = [i for i in range(18, 24)]
        y_val_int = [i for i in range(22, 28)]

        # x_val_int = [i for i in range(17, 25)]
        # y_val_int = [i for i in range(21, 29)]

        for x in x_val_int:
            for y in y_val_int:
                self.intersection.append([x, y])
                # agent = DebugAgents(self.unique_ids, "intersection", self)
                # self.grid.place_agent(agent, (x, y))
                # self.unique_ids += 1

        self.streets = {}

        down_dict = {}
        down_left = []
        down_right = []

        # Down left street
        x_val_int = [18, 19, 20]
        y_val_int = [i for i in range(0, 22)]

        for x in x_val_int:
            for y in y_val_int:
                down_left.append([x, y])

        # Down right street
        x_val_int = [21, 22, 23]
        y_val_int = [i for i in range(0, 22)]

        for x in x_val_int:
            for y in y_val_int:
                down_right.append([x, y])

        down_dict["right"] = down_right
        down_dict["left"] = down_left
        self.streets["down"] = down_dict

        up_dict = {}
        up_left = []
        up_right = []

        # Up left street
        x_val_int = [18, 19, 20]
        y_val_int = [i for i in range(28, 50)]

        for x in x_val_int:
            for y in y_val_int:
                up_left.append([x, y])

        # Up right street
        x_val_int = [21, 22, 23]
        y_val_int = [i for i in range(28, 50)]

        for x in x_val_int:
            for y in y_val_int:
                up_right.append([x, y])

        up_dict["right"] = up_right
        up_dict["left"] = up_left
        self.streets["up"] = up_dict

        left_down = []
        left_up = []
        left_dict = {}

        # Left down street
        x_val_int = [i for i in range(0, 18)]
        y_val_int = [22, 23, 24]

        for x in x_val_int:
            for y in y_val_int:
                left_down.append([x, y])

        # Left up street
        x_val_int = [i for i in range(0, 18)]
        y_val_int = [25, 26, 27]

        for x in x_val_int:
            for y in y_val_int:
                left_up.append([x, y])

        left_dict["up"] = left_up
        left_dict["down"] = left_down
        self.streets["left"] = left_dict

        right_down = []
        right_up = []
        right_dict = {}

        # Right down street
        x_val_int = [i for i in range(24, 50)]
        y_val_int = [22, 23, 24]

        for x in x_val_int:
            for y in y_val_int:
                right_down.append([x, y])

        # Right up street
        x_val_int = [i for i in range(24, 50)]
        y_val_int = [25, 26, 27]

        for x in x_val_int:
            for y in y_val_int:
                right_up.append([x, y])

        right_dict["up"] =  right_up
        right_dict["down"] = right_down
        self.streets["right"] = right_dict

        self.data = mesa.DataCollector(
                {
                    "Waiting time" : IntersectionModel.average_waiting_time,
                    "Succesfull trips" : IntersectionModel.succesful_trips,
                    "Crashes" : IntersectionModel.crashes
                }
        )


    def get_data(self):
        data = {}

        data["cars"] = [ {"pos":[int(agent.pos[0]), int(agent.pos[1])], "name": "Car" if isinstance(agent, Car) else "Ambulance", "id" : agent.unique_id} for agent in self.vh_scheduler.agents]

        data["tf"] = [
            {"pos": [int(agent.pos[0]), int(agent.pos[1])], "status":agent.status}
            for agent in self.vh_scheduler.agents]

        return data


    """ GET TRAFFIC LIGHTS READ  """
    def get_tf_reads(self):
        s_up = [agents for agents in self.tl_scheduler.agents if agents.direction == "up"]
        s_down = [agents for agents in self.tl_scheduler.agents if agents.direction == "down"]
        s_left = [agents for agents in self.tl_scheduler.agents if agents.direction == "left"]
        s_right = [agents for agents in self.tl_scheduler.agents if agents.direction == "right"]

        decide = {}
        count = 0

        for one in s_up:
            res_up = list()

            for i in range(1, 5):
                aux = (one.pos[0], one.pos[1] + i)
                res_up.append(one.model.grid.get_cell_list_contents(aux))

            for val in res_up:
                for stuff in val:
                    if isinstance(stuff, Car) or isinstance(stuff, Ambulance):
                        count += 1

        if (count != 0):
            decide["up"] = count

        count = 0

        for two in s_down:
            res_down = list()

            for i in range(1, 5):
                aux = (two.pos[0], two.pos[1] - i)
                res_down.append(two.model.grid.get_cell_list_contents(aux))

            for val in res_down:
                for stuff in val:
                    if isinstance(stuff, Car) or isinstance(stuff, Ambulance):
                        count += 1

        if (count != 0):
            decide["down"] = count

        count = 0

        for three in s_left:
            res_left = list()

            for i in range(1, 5):
                aux = (three.pos[0] - i, three.pos[1])
                res_left.append(three.model.grid.get_cell_list_contents(aux))

            for val in res_left:
                for stuff in val:
                    if isinstance(stuff, Car) or isinstance(stuff, Ambulance):
                        count += 1

        if (count != 0):
            decide["left"] = count

        count = 0

        for four in s_right:
            res_right = list()

            for i in range(1, 5):
                aux = (four.pos[0] + i, four.pos[1])
                res_right.append(four.model.grid.get_cell_list_contents(aux))

            for val in res_right:
                for stuff in val:
                    if isinstance(stuff, Car) or isinstance(stuff, Ambulance):
                        count += 1

        if (count != 0):
            decide["right"] = count

        if decide == {}:
            return []
        else:
            return sorted(decide, key=decide.get, reverse=True)


    """ STEP FOR SENSOR """
    def get_vel_reads(self):
        s_up = [agents for agents in self.tl_scheduler.agents if agents.direction == "up"]
        s_down = [agents for agents in self.tl_scheduler.agents if agents.direction == "down"]
        s_left = [agents for agents in self.tl_scheduler.agents if agents.direction == "left"]
        s_right = [agents for agents in self.tl_scheduler.agents if agents.direction == "right"]

        decide = {}
        count = 0
        vel = 0

        for one in s_up:
            res_up = list()

            for i in range(7, 18):
                aux = (one.pos[0], one.pos[1] + i)
                res_up.append(self.grid.get_cell_list_contents(aux))

            for val in res_up:
                for stuff in val:
                    if isinstance(stuff, Car) or isinstance(stuff, Ambulance):
                        count += 1
                        vel += stuff.velocity

        if (count != 0):
            decide["up"] = vel/count

        count = 0
        vel = 0

        for two in s_down:
            res_down = list()

            for i in range(5, 18):
                aux = (two.pos[0], two.pos[1] - i)
                res_down.append(self.grid.get_cell_list_contents(aux))

            for val in res_down:
                for stuff in val:
                    if isinstance(stuff, Car) or isinstance(stuff, Ambulance):
                        count += 1
                        vel += vel

        if (count != 0):
            decide["down"] = vel/count

        count = 0
        vel = 0

        for three in s_left:
            res_left = list()

            for i in range(5, 18):
                aux = (three.pos[0] - i, three.pos[1])
                res_left.append(self.grid.get_cell_list_contents(aux))

            for val in res_left:
                for stuff in val:
                    if isinstance(stuff, Car) or isinstance(stuff, Ambulance):
                        count += 1
                        vel += vel

        if (count != 0):
            decide["left"] = vel/count

        count = 0
        vel = 0

        for four in s_right:
            res_right = list()

            for i in range(5, 18):
                aux = (four.pos[0] + i, four.pos[1])
                res_right.append(self.grid.get_cell_list_contents(aux))

            for val in res_right:
                for stuff in val:
                    if isinstance(stuff, Car) or isinstance(stuff, Ambulance):
                        count += 1
                        vel += vel

        if (count != 0):
            decide["right"] = vel/count

        if decide == {}:
            return []
        else:
            return sorted(decide, key=decide.get, reverse=True)


    # SPAWN VEHICLES
    def spawnVehicles(self):
        random.shuffle(self.spawn)

        for location in self.spawn:

            if self.curr_cars == self.max_cars:
                return

            spawn_prob = round(random.uniform(0, 1), 2)

            if spawn_prob >= .03:

                check_spawn = self.grid.get_cell_list_contents(tuple(location))

                if len(check_spawn) > 0: # if there is already an agent in the location I want to use to spawn
                    continue # check next location
                else:
                    x, y = location # extract the location
                    spawn_pos = [x, y]
                    agent = Car(self.unique_ids, self) # creates agent

                    # Set crazy timer
                    agent.crazy_timer = random.randrange(20, 30)

                    self.vh_scheduler.add(agent) # adds agent to scheduler
                    self.grid.place_agent(agent, (x, y))
                    status_prob = round(random.uniform(0, 1), 2)
                    status_debug = ""
                    # Set agent status
                    if .80 < status_prob <= .98:
                        if self.debug is True:
                            status_debug = "[ pressured ]"
                        agent.status = 1
                        agent.velocity = 1
                    elif status_prob > .98:
                        if self.debug is True:
                            status_debug = "[ desesperated ]"
                        agent.status = 2
                        agent.velocity = 2
                    else:
                        if self.debug is True:
                            status_debug = "[ normal ]"
                            agent.velocity = 1
                    # Set cars' destination
                    copy_of_dispawn = self.dispawn
                    random.shuffle(copy_of_dispawn)
                    for destination in copy_of_dispawn:
                        if (get_distance(location, destination) < 20):
                            continue
                        else:
                            agent.final_des = destination
                            for key in self.streets:
                                for vals in self.streets[key]:
                                    for side in self.streets[key][vals]:
                                        if agent.final_des == side:
                                            agent.final_street = key # this will be which street
                                            agent.final_side = vals # this will be which side of the street
                            # get current street name
                            for key in self.streets:
                                for vals in self.streets[key]:
                                    for side in self.streets[key][vals]:
                                        if spawn_pos == side:
                                            agent.curr_street = key # this will be which street
                                            agent.curr_side = vals # this will be which side of the street
                            break
                    # if self.debug is True:
                    #     print(f" [[ Car ]] spawned at ({x}. {y}) with status of {status_debug}")
                    #     print(f"    ::- Will go to {agent.final_des}")
                    self.unique_ids += 1
                    self.curr_cars += 1

            elif spawn_prob < .03:

                check_spawn = self.grid.get_cell_list_contents(tuple(location))

                if len(check_spawn) > 0: # if there is already an agent in the location I want to use to spawn
                    continue # check next location
                else:
                    x, y = location # extract the location
                    spawn_pos = [x, y]

                    # SPAWNING TAIL
                    tail = AmbulanceTail(self.unique_ids, self) # creates agent
                    self.unique_ids += 1
                    agent = Ambulance(self.unique_ids, tail, self) # creates agent
                    tail.head = agent

                    self.vh_scheduler.add(agent) # adds agent to scheduler

                    self.grid.place_agent(tail, (x, y))
                    self.grid.place_agent(agent, (x, y))

                    # Set cars' destination
                    copy_of_dispawn = self.dispawn
                    random.shuffle(copy_of_dispawn)

                    for destination in copy_of_dispawn:
                        if (get_distance(location, destination) < 20):
                            continue
                        else:
                            agent.final_des = destination

                            for key in self.streets:
                                for vals in self.streets[key]:
                                    for side in self.streets[key][vals]:

                                        if agent.final_des == side:
                                            agent.final_street = key # this will be which street
                                            agent.final_side = vals # this will be which side of the street

                            # get current street name
                            for key in self.streets:
                                for vals in self.streets[key]:
                                    for side in self.streets[key][vals]:

                                        if spawn_pos == side:
                                            agent.curr_street = key # this will be which street
                                            agent.curr_side = vals # this will be which side of the street
                            break

                    # if self.debug is True:
                    #     print(f" [[ Ambulance ]] spawned at ({x}. {y}) with status of {agent.status}")
                    #     print(f"    ::- Will go to {agent.final_des}")

                    self.unique_ids += 1
                    self.curr_cars += 1

        self.vh_scheduler.step()

    def check_inter_empty(self):
        vehicles = [agents for agents in self.vh_scheduler.agents]

        for location in self.intersection:
            for vehicle in vehicles:
                x_v, y_v = vehicle.pos
                compare = [x_v, y_v]

                if compare == location:
                    return False
        return True


    def step(self):

        # if self.debug is True:
        #     print(f"The max number of cars is {self.max_cars}")
        #     print(f"The current number of cars is {self.curr_cars}")

        if self.curr_cars < self.max_cars:
            self.spawnVehicles()
            return self.get_data()

        if self.tf_cycle is True: # if there is a cycle active
            if self.time == 15:
                # Set current traffic light in green to yellow
                self.yellow_light = True
                self.tl_scheduler.step() # move traffic lights
                self.time += 1

            elif self.time == 25: # max time
                # Do while intersection is not empty

                if len(self.prio) == 1:
                    self.time = 0
                    self.tf_cycle = False
                    self.yellow_light = False
                    self.tl_scheduler.step() # move vehicles
                    return
                else:
                    self.prio.pop(0)
                    self.time = 0
                    self.yellow_light = False
                    self.tl_scheduler.step()

            else:
                self.vh_scheduler.step() # move vehicles
                self.time += 1
        else:
            self.prio = self.get_tf_reads() # traffic has priority

            if self.prio == []: # if no traffic then get vel reads
                self.prio = ['up', 'right', 'down', 'left']
            else:
                self.tf_cycle = True

            self.tl_scheduler.step()
            self.vh_scheduler.step()

        self.data.collect(self)
        return self.get_data()

    @staticmethod
    def average_waiting_time(model):
        return np.average([agent.waiting_time for agent in model.vh_scheduler.agents])

    @staticmethod
    def succesful_trips(model):
        return model.succesfull

    @staticmethod
    def crashes(model):
        return model.crashes_num
