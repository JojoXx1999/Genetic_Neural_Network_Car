using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

/// <summary>
/// Class used to give the AI controll over the car
/// affects everything to do with the car, such as
/// movment, but also the input sensors telling the
/// car what it is able to 'see' in the world
/// </summary>
[RequireComponent(typeof(NeuralNetwork))]
public class CarController : MonoBehaviour
{

    //Where the car should start at, and respawn at each time
    private Vector3 startPosition, startRotation;

    //Is training?
    public bool train = false;

    //Set up a network object
    private NeuralNetwork network;

    //Time scale to determine how fast the game will run
    public int timeScale = 1;

    //Set up the architecture of the neural network, 3 input neurons, 3 hidden layer neurons and 2 output neurons
    [Header("Network")]
    public int[] Layers = { 3, 7, 2 };

    //Set the acceleration and rotation of the car to a value between -1 and 1
    [Range(-1f, 1f)]
    public float acceleration, rotation;

    //set timer to 0
    public float timePassed = 0f;

    //Variables to be used for caculating the fitness of the individual
    [Header("Fitness")]
    public float totalFitness;
    public float distanceI = 1.5f; //how important is distance travelled
    public float avgSpeedI = 0.1f; //how important is speed
    public float sensorsI = 0.2f; // how important to be in middle

    //Values for calculating fitness
    private Vector3 lastPosition;
    private float totalDistance, avgSpeed;

    //Raycasts to be used as the inputs for the NN
    private float[] Sensors = new float[3];

    //Stores the file locations of the external files
    public string fullPath, fullTrainingPath;

    //if using checkpoints
    string previousCheckPoint = "Last";

    //only skip one individual
    bool pressOnce;

    /// <summary>
    /// Set up car at start
    /// </summary>
    private void Awake()
    {
        //Paths to the external files used for saving and loading data
        fullPath = Application.dataPath + "/" + "Saved Best Network.dat";
        fullTrainingPath = Application.dataPath + "/" + "Saved Training Network.dat";

        //Set slightly above the ground with a 90 degree rotation
        startPosition = new Vector3(0, 1, 0);
        startRotation = transform.eulerAngles;
        transform.Rotate(0, 90, 0);
        pressOnce = false;

    }

    /// <summary>
    /// Things that must happen constantly
    /// will never run more than 50 times per second
    /// </summary>
    private void FixedUpdate()
    {
        //set up the sensors
        InputSensors();
        //get the last position recorded for the cube
        lastPosition = transform.position;

        //Set input to the sensors
        float[] inputs = { Sensors[0], Sensors[1], Sensors[2] };

        //Get the output layer from the neural network
        float[] outputs = network.Output(inputs);

        //Set the values in the output layer to the acceleration and rotation of the cube
        acceleration = outputs[0]; rotation = outputs[1];

        //Feed these values into the move car function
        Move(acceleration, rotation);

        //Increment the time passed
        timePassed += Time.deltaTime;

        //How fast the game runs
        Time.timeScale = timeScale;

        //Calculate the fitness of the current individual
        CalculateFitness();

        //Get user input
        UserInput();
    }

    /// <summary>
    /// Reset the car
    /// </summary>
    public void Reset()
    {
        //Set everything to their initial values
        timePassed = 0f;
        totalDistance = 0f;
        avgSpeed = 0f;
        totalFitness = 0f;

        lastPosition = startRotation;
        transform.position = startPosition;
        transform.eulerAngles = startRotation;
        transform.Rotate(0, 90, 0);
    }

    /// <summary>
    /// If the cube has collided with an object
    /// </summary>
    /// <param name="collision"></param>
    private void OnCollisionEnter(Collision collision)
    {
        //If the cube has hit a wall
        if (collision.gameObject.transform.parent.gameObject.name == "Walls")
            Death(); //Kill this individual

    }

    /// <summary>
    /// Move the cube
    /// </summary>
    private Vector3 input;
    public void Move(float acceleration, float rotation)
    {
        //Integrate the position of the cube
        input = Vector3.Lerp(Vector3.zero,new Vector3(0, 0, acceleration*11.5f), 0.05f);
        input = transform.TransformDirection(input);
        transform.position += input;
        //Turn the cube smoothly
        transform.eulerAngles += new Vector3(0, (rotation * 90) * 0.05f, 0);
    }


    /// <summary>
    /// Set up raycasts to act as inputs to the neural net
    /// </summary>
    private void InputSensors()
    {
        //Create 3 vectors coming from the cube
        Vector3 right = (transform.forward + transform.right);
        Vector3 forward = (transform.forward);
        Vector3 left = (transform.forward - transform.right);

        //Create a ray
        Ray ray = new Ray(transform.position, right);
        RaycastHit hit;

        //If this ray hits something
        if (Physics.Raycast(ray, out hit))
        {
            Sensors[0] = System.Math.Abs(hit.distance); //Normalise value for NN
            Debug.DrawLine(ray.origin, hit.point, Color.red); //Draw ray
        }

        //Set ray
        ray.direction = forward;
        
        //if this ray hits anything
        if (Physics.Raycast(ray, out hit))
        {
            Sensors[1] = System.Math.Abs(hit.distance); //Normalise value for NN
            Debug.DrawLine(ray.origin, hit.point, Color.red); //Draw ray
        }

        //Set ray
        ray.direction = left;

        //if this ray hits anything
        if (Physics.Raycast(ray, out hit))
        {
            Sensors[2] = System.Math.Abs(hit.distance); //Normalise value for NN
            Debug.DrawLine(ray.origin, hit.point, Color.red); //Draw ray
        }
    }


    /// <summary>
    /// Calculate the fitness of this neural network
    /// </summary>
    private void CalculateFitness()
    {
        //Calculate how far the cube has moved
        totalDistance += Vector3.Distance(transform.position, lastPosition);
        //Calculate the average speed of the cube
        avgSpeed = totalDistance / timePassed;

        //Calculate the overall fitness using the distance, speed, sensors and how important each thing is
        totalFitness = (((Sensors[0] + Sensors[1] + Sensors[2]) / 3) * sensorsI) + (totalDistance * distanceI) + (avgSpeed * avgSpeedI);

        //If the cube hasn't improved much
        if (timePassed > 20 && totalFitness < 50)
        {
            //kill the individual
            Death();
        }

        //If the cube is doing really well
        if (train == true)
        {
            if (totalFitness >= 4000)
            {
                //kill the individual
                Death();
            }
        }
        

    }

    /// <summary>
    /// Convert values to between 0 and 1
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    float sigmoid(float s)
    {
        return 1 / (1 + Mathf.Exp(-s));
    }

    /// <summary>
    /// Reset the cube
    /// </summary>
    /// <param name="net"></param>
    public void ResetWithNetwork(NeuralNetwork net)
    {
        network = net;
        Reset();
    }

    //Get user input
    private void UserInput()
    {
        //If user presses space kill the current individual
        //Only allow it to register one press at a time to stop
        //it killing multiple individuals at one time
        if (Input.GetKeyDown("space")&& pressOnce == false)
        {
            pressOnce = true;
            Death();
        }
        else if (Input.GetKeyUp("space"))
        {
            pressOnce = false;
        }
    }

    //Kill the current individual
    private void Death()
    {
        //call the death function in the EvolutionaryManager script
        GameObject.FindObjectOfType<EvolutionaryManager>().Death(totalFitness, network);

        //reset the cube
        Reset();
    }
}
