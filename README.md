# Genetic_Neural_Network_Car
Self-driving car that learns to drive around a track using a genetic neural network
This project has two scenes, train and trained.

Train will start a new neural network and train it through genetic mutation, each new neural network
that performs better than the current best network will have its weights saved to an external file
The trained scene will load the weights from this external file.

Scenes can be found in Assets/Scenes
The external files for saving or loading the neural networks are 'Saved Best Network.dat' and
'Saved Training Network.dat' both in Assets. The first number in Saved Best Network is the fitness of the network
each proceeding number is one weight in the neural network.

The scripts for this project are found in Assets/Scripts

CameraFollow.cs updates the camera to follow the car as it travels around the track

CarController.cs creates the raycasts which are used as the inputs for the Neural Network, these
inputs are used to calculate the fitness of the network. Finally this script also gives the AI the ability
to move the car model, accelerating forwards or backwards and rotation.

NeuralNetwork.cs sets up the structure of the Neural Network AI and sets up weight connections between 
each neuron. This script gives a random chance of a number of different mutation occuring to each
weight in the network. The weights are written to or loaded from the external file.

EvolutionaryManager.cs creates each generation, sets a mutation rate which effects how often mutations
will occur. The main purpose of this script is to cross over the data from the best 10 networks from
a single generation to create child networks for the next. 

More information can be found here: http://www.jodieduff.co.uk/self_driving_ai.html


