using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;

public class NeuralNetwork
{
    private float[][] neurons;
    private float[][] biases;
    private float[][][] weights;
    private static int[] layers = {10, 20, 3};

    public NeuralNetwork()
    {
        init();
    }

    private void init()
    {
        var list = new List<float[]>();
        
        for (int i = 0; i < layers.Length; i++)
            list.Add(new float[layers[i]]);
        
        neurons = list.ToArray();

        initializeWeights();
        initializeBiases();
    }

    private float xavierInitialization(int layerSize)
    {
        return UnityEngine.Random.Range(-1f, 1f);
    }

    private void initializeWeights()
    {
        var list = new List<float[][]>();

        for (int i = 0; i < layers.Length - 1; i++)
        {
            var subList = new List<float[]>();

            for (int j = 0; j < layers[i]; j++)
            {
                subList.Add(new float[layers[i + 1]]);
                for (int k = 0; k < layers[i + 1]; k++)
                    subList[j][k] = xavierInitialization(layers[i]);
            }
            list.Add(subList.ToArray());
        }

        weights = list.ToArray();
    }

    private void initializeBiases()
    {
        var list = new List<float[]>();

        for (int i = 1; i < layers.Length; i++)
        {
            list.Add(new float[layers[i]]);

            for (int j = 0; j < layers[i]; j++)
                list[i - 1][j] = UnityEngine.Random.Range(-1f, 1f);
        }

        biases = list.ToArray();
    }

    public float[] forwardPass(float[] inputs)
    {
        neurons[0] = inputs;

        for (int i = 1; i < layers.Length; i++)
        {
            for (int j = 0; j < neurons[i].Length; j++)
            {
                float value = 0;
                for (int k = 0; k < neurons[i - 1].Length; k++)
                {
                    value += weights[i - 1][k][j] * neurons[i - 1][k];
                }

                if (i != 1)
                    value += biases[i - 1][j];
                
                neurons[i][j] = (float)Math.Tanh(value);
            }
        }

        return neurons[layers.Length - 1];
    }

    public void loadFromFile()
    {
        string weights_path = "Assets/Scripts/Weights.txt", biases_path = "Assets/Scripts/Biases.txt";
        TextReader weightsReader = new StreamReader(weights_path);
        TextReader biasesReader = new StreamReader(biases_path);

        var list = new List<float[][]>();

        for (int i = 0; i < layers.Length - 1; i++)
        {
            var subList = new List<float[]>();

            for (int j = 0; j < layers[i]; j++)
            {
                subList.Add(new float[layers[i + 1]]);
                for (int k = 0; k < layers[i + 1]; k++)
                    subList[j][k] = float.Parse(weightsReader.ReadLine());
            }
            list.Add(subList.ToArray());
        }
        weightsReader.Close();

        weights = list.ToArray();

        var biaseslist = new List<float[]>();

        for (int i = 1; i < layers.Length; i++)
        {
            biaseslist.Add(new float[layers[i]]);

            for (int j = 0; j < layers[i]; j++)
                biaseslist[i - 1][j] = float.Parse(biasesReader.ReadLine());
        }

        biases = biaseslist.ToArray();
    }

    public void copy(NeuralNetwork network)
    {
        for (int i = 0; i < weights.Length; i++)
        {
            for (int j = 0; j < weights[i].Length; j++)
            {
                for (int k = 0; k < weights[i][j].Length; k++)
                {
                    weights[i][j][k] = network.weights[i][j][k];
                }
            }
        }

        for (int i = 0; i < biases.Length; i++)
        {
            for (int j = 0; j < biases[i].Length; j++)
            {
                biases[i][j] = network.biases[i][j];
            }
        }
    }

    public void mutate()
    {
        for (int i = 0; i < weights.Length; i++)
        {
            for (int j = 0; j < weights[i].Length; j++)
            {
                for (int k = 0; k < weights[i][j].Length; k++)
                {
                    weights[i][j][k] += UnityEngine.Random.Range(-1f, 1f);
                }
            }
        }

        for (int i = 0; i < biases.Length; i++)
        {
            for (int j = 0; j < biases[i].Length; j++)
            {
                biases[i][j] += UnityEngine.Random.Range(-1f, 1f);
            }
        }
    }

    public void saveBestWeightsAndBiases()
    {
        string weights_path = "Assets/Script/Weights.txt", biases_path = "Assets/Script/Biases.txt";
        File.Create(weights_path).Close();

        StreamWriter writer = new StreamWriter(weights_path, false);

        foreach (float[][] weights_layer in weights)
        {
            foreach (float[] weights in weights_layer)
            {
                foreach (float value in weights)
                {
                    writer.WriteLine(value);
                }
            }
        }

        File.Create(biases_path).Close();

        writer = new StreamWriter(biases_path, false);

        foreach (float[] biases_layer in biases)
        {
            foreach (float bias in biases_layer)
            {
                writer.WriteLine(bias);
            }
        }

    }
}