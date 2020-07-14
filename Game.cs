using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System;
using System.Collections.Generic;

namespace GolgedarEngine
{
   public class Game
   {
      readonly Color BACKGROUND_COLOR = new Color(25, 125, 25);
      private readonly Dictionary<Keyboard.Key, bool> checkKeyboard = new Dictionary<Keyboard.Key, bool>();

      public Game()
      {
         ActiveGame = this;
         Clock = new Clock();

         foreach (Keyboard.Key key in Enum.GetValues(typeof(Keyboard.Key)))
            checkKeyboard.TryAdd(key, false);
      }

      private void Initialization()
      {
         Window = new RenderWindow(new VideoMode(1920, 1080), "SFML window");
         Window.SetVisible(true);
         Window.SetVerticalSyncEnabled(true);

         Window.SetKeyRepeatEnabled(true);
         Window.Closed += new EventHandler(WindowClosed);
         Window.KeyPressed += new EventHandler<KeyEventArgs>(KeyPressed);
         Window.KeyReleased += new EventHandler<KeyEventArgs>(KeyReleased);

         Clock.Restart();
      }
      public void Run()
      {
         Initialization();
         GameStart();
         MainLoop();
         GameEnd();
      }

      // Phases
      private void Draw()
      {
         foreach (GameObject gameObject in ActiveRoom.Instances_SortedByDepth.Values)
            gameObject.Draw();
      }
      private void GameEnd()
      {
      }
      private void GameStart()
      {
      }
      private void MainLoop()
      {
         while (Window.IsOpen && ActiveRoom != null)
         {
            DeltaTime = Clock.ElapsedTime.AsMilliseconds();
            Clock.Restart();

            foreach (GameObject gameObject in ActiveRoom.Instances_SortedByCreation.Values)
               gameObject.Loop();

            Window.Clear(BACKGROUND_COLOR);
            Draw();

            Window.DispatchEvents();
            Window.Display();
         }
      }

      // Events
      private void WindowClosed(object sender, EventArgs e)
      {
         GameEnd();
         Window.Close();
      }
      private void KeyPressed(object sender, KeyEventArgs e)
      {
         checkKeyboard[e.Code] = true;
      }
      private void KeyReleased(object sender, KeyEventArgs e)
      {
         checkKeyboard[e.Code] = false;
      }

      public bool CheckKey(Keyboard.Key key)
      {
         return checkKeyboard.GetValueOrDefault(key, false);
      }

      // Properties
      public int DeltaTime { get; private set; }
      public RenderWindow Window { get; set; }
      public Room ActiveRoom { get; set; }
      public Clock Clock { get; }
      public static Game ActiveGame { get; internal set; }
   }
}