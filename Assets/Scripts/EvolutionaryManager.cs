using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EvolutionaryManager : MonoBehaviour
{

    //UI
    public UIController display;

    //average fitness
    private float avgFitness = 0f;

    //Bools to decide if the system should train or not
    public bool train = true;
    private bool ImportPreviousTrainingSession = true;

    //Gmame object to the car controller
    [Header("Car Controller")]
    public CarController controller;

    //Genetic algorithm controlls,
    [Header("GA Info")]
    public int startPopulation = 50;
    public int currentGeneration;
    public int currentGenome = 1;
    public Text genText, popText;

    //Rate of mutation
    public float mutationRate = 0.01f;

    //Cross over parameters
    [Header("Crossover")]
    public int bestAgentSelection = 10;
    private int worseAgentSelection = 0;

    public int numberToCrossover;
    private bool runOnce;

    //List to store genes of the generation
    private List<int> gene = new List<int>();
    private int naturallySelected;

    //Set up population and variables for saving
    public NeuralNetwork[] population;
    private NeuralNetwork bestNetwork;
    int bestIndividual = 0, bestGen = 0;
    public float bestFitness = 0;


    //Run on start up
    private void Start()
    {
        display = new UIController();
        //Set up a new neural network and set best network to it
        int[] inputs = { 3, 7, 2 };
        bestNetwork = new NeuralNetwork(inputs);

        //Set up the population
        CreatePopulation();

        //Load the best fitness from an external file
         bestNetwork.Load(controller.fullPath);
         bestFitness = bestNetwork.bestFitness;

        //Display information to the screen
        controller.train = train;
        display.totalPop = population.Length;
        display.generation = genText;
        display.population = popText;

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="newPopulation"></param>
    /// <param name="startingIndex"></param>
    private void Fill(NeuralNetwork[] newPopulation, int startingIndex)
    {
        //3 input neurons, 3 hidden layer neurons, 2 output neurons
        int[] inputs = { 3, 7, 2 };

        //while still members in the population
        while (startingIndex < startPopulation)
        {
            //Give them a neural network
            newPopulation[startingIndex] = new NeuralNetwork(inputs);
            startingIndex++;
        }
    }

    /// <summary>
    /// Create a population
    /// </summary>
    private void CreatePopulation()
    {
        //Create a neural network for each individual in the population
        population = new NeuralNetwork[startPopulation];

        //Set the weights in the networks to random values
        Fill(population, 0);

        //Reset
        ResetGenome();

        //If not training the system
        if (train == false)
        {
            //For each member in the population
            for (int i = 0; i < population.Length; i++)
            {
                //Set their neural network to the best saved neural network
                population[i] = bestNetwork;
            }
        }

    }

    /// <summary>
    /// Reset
    /// </summary>
    private void ResetGenome()
    {
        controller.ResetWithNetwork(population[currentGenome]);
    }

    /// <summary>
    /// Kill the individual
    /// </summary>
    /// <param name="fitness"></param>
    /// <param name="net"></param>
    public void Death(float fitness, NeuralNetwork net)
    {
        display.Gen = currentGeneration;
        if (train == true) display.Pop = currentGenome + 1;
        else display.Pop = currentGenome;
        display.changeDisplay();

        //If there are still more individuals in the generation
        if (currentGenome < population.Length-1)
        {
            //get the fitness of the individual
            population[currentGenome].fitness = fitness;
            avgFitness += fitness;

            //If the fitness is better than the sotres best fitness and the system is training
            if (fitness > bestFitness && train == true)
            {
                //Set best fitness
                bestFitness = fitness;
                bestIndividual = currentGenome;
                bestGen = currentGeneration;
                
                //get the best network
                bestNetwork = population[bestIndividual];
                //Save the best network to an external file
                bestNetwork.Save(controller.fullPath);
               
                if (runOnce == true) runOnce = false;
            }
            else if (train == false && runOnce == false)
            {
                runOnce = true;
                Debug.Log("Loading");
            }
            currentGenome++;
            ResetGenome();
        }
        else
        {
            //Move to the next generation
            RePopulate();

            if (train == false)
            {
                runOnce = true;
                Debug.Log("Loading");
                bestNetwork.Load(controller.fullPath);
                //LoadNetwork(controller.fullPath);
            }
        }
    }


    /// <summary>
    /// Create the next generation
    /// </summary>
    private void RePopulate()
    {
        //Reset values
        gene.Clear();
        currentGeneration++;
        naturallySelected = 0;
        currentGenome = 1;
        avgFitness /= 85;
        avgFitness = 0f;

        //If training
        if (train == true)
        {
            //Put the population in order
            SortPopulation();

            //Create new networks by mixing networks from parents
            NeuralNetwork[] newPopulation = BestPopulation();
            CrossOver(newPopulation);
            Mutate(newPopulation);

            
            Fill(newPopulation, naturallySelected);
            population = newPopulation;
        }
        else //if not training
        {
            //Set every memebers neural network to the best neural network
            for (int i = 0; i < population.Length; i++)
            {
                population[i] = bestNetwork;
            }
        }

        //reset individual
        ResetGenome();
        
    }

    //cross over genes
    private void CrossOver(NeuralNetwork[] newPopulation)
    {
        for (int i = 0; i < numberToCrossover; i+=2)
        {
            int parent1 = i, parent2 = i + 1;

            if (gene.Count >= 1)
            {
                for (int j = 0; j < 85; j++)
                {
                    parent1 = gene[Random.Range(0, gene.Count)];
                    parent2 = gene[Random.Range(0, gene.Count)];

                    if (parent1 != parent2)
                        break;
                }
            }
            int[] input = { 3, 7, 2 };

            //Create children networks
            NeuralNetwork Child1 = new NeuralNetwork(input);
            NeuralNetwork Child2 = new NeuralNetwork(input);

            Child1.fitness = 0;
            Child2.fitness = 0;

            //for all the weights
            for (int j = 0; j < Child1.weights.Length; j++)
            {
                //Randomly pick which genes come from which parent
                if (Random.Range(0f, 1f) < 0.5f)
                {
                    Child1.weights[j] = population[parent1].weights[j];
                    Child2.weights[j] = population[parent2].weights[j];
                }
                else
                {
                    Child2.weights[j] = population[parent1].weights[j];
                    Child1.weights[j] = population[parent2].weights[j];
                }
            }
            
            //add child to population
            newPopulation[naturallySelected] = Child1;
            naturallySelected++;

            //add child to population
            newPopulation[naturallySelected] = Child2;
            naturallySelected++;
            
            

        }
    }

    /// <summary>
    /// Mutate the created individual
    /// </summary>
    /// <param name="newPopulation"></param>
    private void Mutate(NeuralNetwork[] newPopulation)
    {
        for (int i = 0; i < naturallySelected; i++)
        {
            //for all the weights
            for (int j = 0; j < newPopulation[i].weights.Length; j++)
            {
                //if chosen to mutate
                if (Random.Range(0f, 1f) < mutationRate)
                {
                    //Mutate the idividual
                    newPopulation[i].Mutate(); 
                }
            }
        }
    }

    /// <summary>
    /// Pick the best neural networks
    /// </summary>
    /// <returns></returns>
    private NeuralNetwork[] BestPopulation()
    {
        NeuralNetwork[] newPopulation = new NeuralNetwork[startPopulation];

        //For the number of top networks should be taken
        for (int i = 0; i < bestAgentSelection; i++)
        {
            //set to the copied neural network
            newPopulation[naturallySelected] = population[i].copy();
            newPopulation[naturallySelected].fitness = 0;
            naturallySelected++;

            //Chance of reproducing is greater when the fitness is bigger
            int x = Mathf.RoundToInt(population[i].fitness * 10);

            for (int j = 0; j < x; j++)
            {
                gene.Add(i);
            }
        }

        return newPopulation;
    }

    /// <summary>
    /// Sort the population
    /// </summary>
    private void SortPopulation()
    {
        for (int i = 0; i < population.Length; i++)
        {
            for (int j = 0; j < population.Length; j++)
            {
                for (int k = 0; k < population.Length; k++)
                {
                    NeuralNetwork temp = population[i];
                    population[i] = population[j];
                    population[j] = temp;
                }
            }
        }
    }

}

