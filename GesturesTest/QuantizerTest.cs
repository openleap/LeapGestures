using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LeapGestures.Logic;
using System.Linq;
using LeapGestures.Events;

namespace GesturesTest
{
    [TestClass]
    public class QuantizerTest
    {
        [TestMethod]
        public void CircleTest()
        {
            var circle = Circle();

            var quantizer = new Quantizer(8);

            quantizer.trainCenteroids(circle);

            var sequence = quantizer.getObservationSequence(circle);
        }

        private Gesture Circle()
        {
            var gesture = new Gesture();

            var steps = Enumerable.Range(1, 100)
                .Select(i => 2 * Math.PI * i / 100.0)
                .Select(i => new AccelerationVector(-Math.Sin(i), -Math.Cos(i), -Math.Sin(i)));

            foreach (var s in steps)
            {
                gesture.add(s);
            }

            return gesture;
        }
    }
}
