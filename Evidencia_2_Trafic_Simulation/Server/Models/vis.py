from mesa.visualization.modules import CanvasGrid
from mesa.visualization.ModularVisualization import ModularServer
from mesa.visualization.UserParam import UserSettableParameter

# from inter_model import *
# from normal import *
# from traffic import *
# from vel import *

simulation_params = {
        "max_cars_num":UserSettableParameter(
                "slider",
                "Numero de carros simultaneos",
                10, # default
                10, # min
                50, # max
                1, # step
                description="Elija el numero de carros que debe de haber maximo en la simulacion"
            ),
        "debug":UserSettableParameter(
                "checkbox",
                "Debug",
                True,
                description="Elija si mostrar debug o no"
            )
}

def agent_portrayal(agent):
    portrayal = {"Shape": "circle",
                 "Filled": "true",
                 "Layer": 0,
                 "Color": "red",
                 "r": 0.5}

    if isinstance(agent, Sidewalk):
        portrayal["Color"] = "brown"
        portrayal["Layer"] = 0
        portrayal["r"] = 0.3
    elif isinstance(agent, Car):
        portrayal["Color"] = "brown"
        portrayal["Layer"] = 1
        portrayal["r"] = 0.9
    elif isinstance(agent, Ambulance):
            portrayal["Color"] = "orange"
            portrayal["Layer"] = 1
            portrayal["r"] = 0.5
    elif isinstance(agent, AmbulanceTail):
            portrayal["Color"] = "orange"
            portrayal["Layer"] = 1
            portrayal["r"] = 0.5
    elif isinstance(agent, TrafficLight):
        if (agent.status == 1): # if stop in step
            portrayal["Color"] = "red"
            portrayal["Layer"] = 1
            portrayal["r"] = 0.5
        elif (agent.status == 2): #
            portrayal["Color"] = "yellow"
            portrayal["Layer"] = 1
            portrayal["r"] = 0.5
        elif (agent.status == 3): #
            portrayal["Color"] = "green"
            portrayal["Layer"] = 1
            portrayal["r"] = 0.5
        else:
            portrayal["Color"] = "black"
            portrayal["Layer"] = 1
            portrayal["r"] = 0.5
    elif isinstance(agent, DebugAgents):
        if (agent.model.debug is True):
            if (agent.status == "dispawn"):
                portrayal["Color"] = "red"
                portrayal["Layer"] = 1
                portrayal["r"] = 0.5
            elif (agent.status == "spawn"):
                portrayal["Color"] = "green"
                portrayal["Layer"] = 1
                portrayal["r"] = 0.5
            elif (agent.status == "intersection"):
                portrayal["Color"] = "orange"
                portrayal["Layer"] = 1
                portrayal["r"] = 0.5
            elif (agent.status == "street"):
                portrayal["Color"] = "blue"
                portrayal["Layer"] = 1
                portrayal["r"] = 0.1
        else:
            portrayal["Color"] = "brown"
            portrayal["Layer"] = 1
            portrayal["r"] = 0


    return portrayal

grid = mesa.visualization.CanvasGrid(agent_portrayal, 50, 50, 500, 500)

chart = mesa.visualization.ChartModule(
    [
        {"Label": "Succesfull trips", "Color":"green"},
    ],
    canvas_height=300,
    data_collector_name="data"
)

chart2 = mesa.visualization.ChartModule(
    [
        {"Label": "Waiting time", "Color":"red"},
    ],
    canvas_height=300,
    data_collector_name="data"
)

chart3 = mesa.visualization.ChartModule(
    [
        {"Label": "Crashes", "Color":"red"},
    ],
    canvas_height=300,
    data_collector_name="data"
)

server = mesa.visualization.ModularServer(
        IntersectionModel, [grid, chart, chart2, chart3], "Intersection model", simulation_params)

server.port = 8521 # The default
server.launch()
