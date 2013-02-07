/*
 * wiigee - accelerometerbased gesture recognition
 * Copyright (C) 2007, 2008, 2009 Benjamin Poppinga
 * 
 * Developed at University of Oldenburg
 * Contact: wiigee@benjaminpoppinga.de
 * 
 * LeapGestures - derivate work of wiigee for the Leap
 * Copyright 2013 Craig Schwartz
 *
 * This file is part of LeapGestures.
 *
 * LeapGestures is free software; you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser General Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser General Public License along
 * with this program; if not, write to the Free Software Foundation, Inc.,
 * 51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.
 */

using Leap;
using LeapGestures.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeapGesturesDemo
{
    class GestureListener : Listener
    {
        private Object thisLock = new Object();

        private Vector lastVelocity = Vector.Zero;

        public TriggeredProcessingUnit Gestures { get; private set; }

        public GestureListener()
        {
            Gestures = new TriggeredProcessingUnit(false);
        }

        private void SafeWriteLine(String line)
        {
            lock (thisLock)
            {
                Console.WriteLine(line);
            }
        }

        public override void OnFrame(Controller controller)
        {
            // Get the most recent frame and report some basic information
            Frame frame = controller.Frame();

            if (!frame.Hands.Empty)
            {
                var pointer = frame.Pointables[0];

                var acceleration = pointer.TipVelocity;

                Gestures.AddData(new double[] { acceleration.x, acceleration.y, acceleration.z });

                lastVelocity = pointer.TipVelocity;
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // Create a sample listener and controller
            GestureListener listener = new GestureListener();
            Controller controller = new Controller();

            listener.Gestures.GestureReceived += Gestures_GestureReceived;

            // Have the sample listener receive events from the controller
            controller.AddListener(listener);

            // Keep this process running until Enter is pressed
            Console.WriteLine("Press Enter to quit...");

            while (true)
            {
                var key = Console.ReadKey(true);

                if (key.Key == ConsoleKey.Enter)
                {
                    break;
                }

                switch (key.Key)
                {
                    case ConsoleKey.D1:
                        listener.Gestures.StartTraining();
                        Console.WriteLine("Training...");
                        break;

                    case ConsoleKey.D2:
                        listener.Gestures.StopTraining();
                        Console.WriteLine("Recorded gesture.");
                        break;

                    case ConsoleKey.D3:
                        var model = listener.Gestures.FinishTrainingSession();
                        Console.WriteLine("Enter a name for this gesture:");
                        var name = Console.ReadLine();
                        model.Name = name;
                        Console.WriteLine(String.Format("Completed training of gesture {0}.", name));
                        break;

                    case ConsoleKey.D4:
                        listener.Gestures.StartRecognition();
                        Console.WriteLine("Recognizing...");
                        break;

                    case ConsoleKey.D5:
                        listener.Gestures.StopRecognition();
                        break;
                }
            }

            // Remove the sample listener when done
            controller.RemoveListener(listener);
            controller.Dispose();
        }

        static void Gestures_GestureReceived(object sender, LeapGestures.Events.GestureEventArgs e)
        {
            if (e.Gesture != null)
            {
                Console.WriteLine(String.Format("Recognized gesture {0} with probability {1}.", e.Gesture.Name, e.Probability.ToString("P4")));
            }
            else
            {
                Console.WriteLine("Gesture not recognized");
            }
        }
    }
}
