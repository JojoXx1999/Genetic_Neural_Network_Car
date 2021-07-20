using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

/// <summary>
/// Class to handle the Neural Network
/// Creates the neural network and handles weights
/// Also saves/loads the current best network from a file
/// </summary>
[System.Serializable]
public class NeuralNetwork
{

    //Network object
    NeuralNetwork net;

    //fitness of the best neural network
    public float bestFitness = 0f;

    //The architecture of the neural net
    public int[] layers;
    public float[][] neurons;
    public float[][][] weights;

    [Range(0f, 1f)] float val = 0.5f;

    int trainingIndex = 1;

    //fitness of the current neural net
    public float fitness;

    //set up variable to be used for (pseudo)random number generation
    private System.Random random;

    private int counter;

    /// <summary>
    /// Neural Net constructor creates the layers
    /// neurons and sets random weights
    /// </summary>
    /// <param name="layers"></param>
    public NeuralNetwork(int[] layers)
    {
        //set number of length of the layers array
        this.layers = new int[layers.Length];

        //set up the all of the layers
        for (int i = 0; i < layers.Length; i++)
        {
            this.layers[i] = layers[i];
        }

        random = new System.Random();

        //Set up up the neurons and weights in the neural net
        InitNeurons();
        InitWeights();

    }

    /// <summary>
    /// Make a copy of the network to be used in EM
    /// </summary>
    /// <returns></returns>
    public NeuralNetwork copy()
    {
        //3 input neurons, 3 hidden layer neurons and 2 output neurons
        int[] input = {3, 7, 2};
        NeuralNetwork n;
        n = new NeuralNetwork(input);

        //return the copied network
        return n;
    }

    /// <summary>
    /// Make a copy of the network
    /// </summary>
    /// <returns></returns>
    public NeuralNetwork(NeuralNetwork copyNetwork)
    {
        //Copy all the layers over
        this.layers = new int[copyNetwork.layers.Length];
        for (int i = 0; i < copyNetwork.layers.Length; i++)
        {
            this.layers[i] = copyNetwork.layers[i];
        }

        //copy the weights and set up neurons
        InitNeurons();
        CopyWeights(copyNetwork.weights);
    }

    /// <summary>
    /// Copy the wight values from the neural network
    /// </summary>
    /// <param name="copyWeights"></param>
    private void CopyWeights(float[][][] copyWeights)
    {
        //Copy all the weights in the neural network
        for (int i = 0; i < weights.Length; i++)
        {
            for (int j = 0; j < weights[i].Length; j++)
            {
                for (int k = 0; k < weights[i][j].Length; k++)
                {
                    weights[i][j][k] = (float)copyWeights[i][j][k];
                }
            }
        }
    }

    /// <summary>
    /// Set up the neurons
    /// </summary>
    private void InitNeurons()
    {
        //this is the neuron matrix
        List<float[]> neuronList = new List<float[]>();

        //Add neurons to the neuron list
        for (int i = 0; i < layers.Length; i++)
        {
            neuronList.Add(new float[layers[i]]);
        }

        //convert to an array
        neurons = neuronList.ToArray();
    }

    //Set up the weights
    private void InitWeights()
    {
        //List of weights
        List<float[][]> weightList = new List<float[][]>();

        //for each layer
        for (int i = 1; i < layers.Length; i++)
        {
            List<float[]> layerWeightList = new List<float[]>();
            //Get the previous neurons
            int neuronsInPreviousLayer = layers[i - 1];

            //repeat for all neurons in current layer
            for (int j = 0; j < neurons[i].Length; j++)
            {
                //set the weight of from the previous neuron
                float[] neuronWeights = new float[neuronsInPreviousLayer];

                //set the weights randomly between 1 and -1
                for (int k = 0; k < neuronsInPreviousLayer; k++)
                {
                    //give random weights to neuron weights
                    //random between -0.5 and 0.5
                    neuronWeights[k] = Random.Range(-0.5f, 0.5f);
                }
                //add the weughts to the list
                layerWeightList.Add(neuronWeights);
            }
            //convert to 2d jagged array
            weightList.Add(layerWeightList.ToArray());
        }
        //convert to 3d jagged array
        weights = weightList.ToArray();
    }

    //Get the outputs from the neural net
    public float[] Output(float[] inputs)
    {
        //Input layer
        for (int i = 0; i < inputs.Length; i++)
        {
            neurons[0][i] = inputs[i];      
        }

        //For every layer, except the input
        for (int i = 1; i < inputs.Length; i++)
        {
            //for every neuron
            for (int j = 0; j < neurons[i].Length; j++)
            {
                //constant bias value 
                float value = 0.5f; 

                //for every neuron in the previous layer
                for (int k = 0; k < neurons[i - 1].Length; k++)
                {
                    //Calculate value
                    value += weights[i - 1][j][k] * neurons[i - 1][k];
                }

                //convert to values between -1 and 1 (activate)
                neurons[i][j] = (float)System.Math.Tanh(value);
            }
        }

        //return the output layer of the neural network
        return neurons[neurons.Length - 1];
    }

    /// <summary>
    /// Mutate the weights of a neural network
    /// </summary>
    public void Mutate()
    {
        
        //For every weight
        for (int i = 0; i < weights.Length; i++)
        {
            for (int j = 0; j < weights[i].Length; j++)
            {
                for (int k = 0; k < weights[i][j].Length; k++)
                {
                    float weight = weights[i][j][k];

                    //mutate weight value 
                    float randomNumber = UnityEngine.Random.Range(0f, 70f);

                    if (randomNumber <= 3f)
                    { //if 1
                      //flip sign of weight
                        weight *= -1f;
                    }
                    else if (randomNumber <= 6f)
                    { //if 2
                      //Pick a random number between -0.5 and 0.5 
                        weight = UnityEngine.Random.Range(-0.5f, 0.5f);
                    }
                    else if (randomNumber <= 9f)
                    { //if 3
                      //randomly pick a number between 0 and 1
                        float factor = UnityEngine.Random.Range(0f, 1f);
                        weight *= factor;
                    }

                    weights[i][j][k] = weight;
                }
            }
        }

    }


    /// <summary>
    /// Save the neural net's fitness into an external file
    /// </summary>
    /// <param name="path"></param>
    public void Save(string path)
    {
        File.Create(path).Close();
        StreamWriter writer = new StreamWriter(path, true);

        //add the fitness of the neural network to the file
        writer.WriteLine(fitness);

        //for the number of weights
        for (int i = 0; i < weights.Length; i++)
            {
                for (int j = 0; j < weights[i].Length; j++)
                {
                    for (int k = 0; k < weights[i][j].Length; k++)
                    {
                       //add weight to file
                        writer.WriteLine(weights[i][j][k]);
                    }
                }
            }
            writer.Close();
     }



    //Load weights from the external file
    public void Load(string path)
    {
        TextReader reader = new StreamReader(path);
        int NumberOfLines = (int)new FileInfo(path).Length;
        string[] ListLines = new string[NumberOfLines];
        int index = 1;

        //for the number of lines in the file
        for (int i = 1; i < NumberOfLines; i++)
        {
            //store the line in a string array
            ListLines[i] = reader.ReadLine();
        }
        reader.Close();

        //If there is data in the file
        if (new FileInfo(path).Length > 0)
        {
            //Get the best network fitness
            bestFitness = float.Parse(ListLines[index]); ;
            index++;

            //for the number of weights
            for (int i = 0; i < weights.Length; i++)
            {
                for (int j = 0; j < weights[i].Length; j++)
                {
                    for (int k = 0; k < weights[i][j].Length; k++)
                    {
                        //set the networks weights to the weights in the file
                        weights[i][j][k] = float.Parse(ListLines[index]); ;
                        index++;
                    }
                }
            }
        }
    }

}
