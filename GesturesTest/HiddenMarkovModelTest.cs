using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LeapGestures.Logic;
using System.Collections.Generic;

namespace GesturesTest
{
    [TestClass]
    public class HiddenMarkovModelTest
    {
        [TestMethod]
        public void Evaluation()
        {
            // based on http://en.wikipedia.org/wiki/Hidden_Markov_model#A_concrete_example

            var model = new HiddenMarkovModel(2, 3);

            model.InitialStateProbabilities = new double[] { 0.6, 0.4 };

            model.TransitionProbabilities = new double[,]
            {
                { 0.7, 0.3 },
                { 0.4, 0.6 }
            };

            model.EmissionProbabilities = new double[,]
            {
                { 0.1, 0.4, 0.5 },
                { 0.6, 0.3, 0.1 }
            };

            int[] observations = new int[] { 1, 2, 0 };
            
            // hand calculate probability by summing probability of this observation sequence
            // over all possible state sequences

            /* State sequences:
             *    0, 0, 0 : Pa = 0.6 * 0.7 * 0.7 = 0.294
             *    0, 0, 1 : Pb = 0.6 * 0.7 * 0.3 = 0.126
             *    0, 1, 0 : Pc = 0.6 * 0.3 * 0.4 = 0.072
             *    0, 1, 1 : Pd = 0.6 * 0.3 * 0.6 = 0.108
             *    1, 0, 0 : Pe = 0.4 * 0.4 * 0.7 = 0.112
             *    1, 0, 1 : Pf = 0.4 * 0.4 * 0.3 = 0.048
             *    1, 1, 0 : Pg = 0.4 * 0.6 * 0.4 = 0.096
             *    1, 1, 1 : Ph = 0.4 * 0.6 * 0.6 = 0.144
             * Sanity check, yes they sum to 1
             */

            /* Output (1, 2, 0) given state sequence:
             *    P(o|a) = 0.4 * 0.5 * 0.1 = 0.02
             *    P(o|b) = 0.4 * 0.5 * 0.6 = 0.12
             *    P(o|c) = 0.4 * 0.1 * 0.1 = 0.004
             *    P(o|d) = 0.4 * 0.1 * 0.6 = 0.024
             *    P(o|e) = 0.3 * 0.5 * 0.1 = 0.015
             *    P(o|f) = 0.3 * 0.5 * 0.6 = 0.09
             *    P(o|g) = 0.3 * 0.1 * 0.1 = 0.003
             *    P(o|h) = 0.3 * 0.1 * 0.6 = 0.018
             */

            // Final probability = P(o|a)*P(a) + ... + P(o|h)*P(h) =
            // 0.00588 + 0.01512 + 0.000288 + 0.002592 + 0.00168 + 0.00432 + 0.000288 + 0.002592 = 0.03276

            var modelProbability = model.getProbability(observations);

            Assert.AreEqual(modelProbability, 0.03276);
        }

        [TestMethod]
        public void Training()
        {
            var model = new HiddenMarkovModel(5, 5);

            model.InitialStateProbabilities = new double[] { 0.1, 0.3, 0.1, 0.4, 0.1 };

            // all emission P should be 0.2
            Assert.IsTrue(model.EmissionProbabilities.Cast<double>().All(e => e == 0.2));

            int[] observations = new int[] { 4, 2, 3, 0, 1, 3, 2 };
            int[] observations2 = new int[] { 3, 4, 0, 2, 1, 1, 4, 1, 3, 2 };

            var sequence = Enumerable.Repeat(observations, 10);
            var sequence2 = Enumerable.Repeat(observations2, 10);

            model.train(sequence);

            Assert.AreEqual(model.EmissionProbabilities[1, 2], 0.25491738788355622);
            Assert.AreEqual(model.EmissionProbabilities[3, 1], 0.087887575284407757);

            model.train(sequence2);

            Assert.AreEqual(model.EmissionProbabilities[1, 2], 0.0096840128278279109);
            Assert.AreEqual(model.EmissionProbabilities[3, 1], 0.10439889167415384);
            Assert.AreEqual(model.TransitionProbabilities[1, 3], 0.35392973024268204);
        }
    }
}
