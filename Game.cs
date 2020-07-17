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
      readonly RectangleShape blackScreen;
      readonly string windowCaption;
      private bool roomChanged;
      private readonly SortedList<Keyboard.Key, bool> checkKeyboard = new SortedList<Keyboard.Key, bool>();
      private readonly SortedList<Keyboard.Key, bool> pressKeyboard = new SortedList<Keyboard.Key, bool>();
      private readonly SortedList<Keyboard.Key, bool> releaseKeyboard = new SortedList<Keyboard.Key, bool>();

      public Game(string windowCaption, RoomData roomData)
      {
         this.windowCaption = windowCaption;

         RoomData = roomData;
         ActiveGame = this;
         Clock = new Clock();
         blackScreen = new RectangleShape();
      }

      public void LoadRoom(string roomName)
      {
         ActiveRoom = new Room(roomName, RoomData);
         blackScreen.FillColor = new Color(0, 0, 0, 255);
         roomChanged = true;
      }
      public void LoadNextRoom()
      {
         try
         {
            LoadRoom(RoomData.GetRooms()[(RoomData.GetRooms().IndexOf(ActiveRoom.RoomName) + 1)]);
         }
         catch (Exception e)
         {
            Console.WriteLine(Global.GetExceptionMessage("The next room does not exists.", e));
         }
      }
      public void LoadPreviousRoom()
      {
         try
         {
            LoadRoom(RoomData.GetRooms()[(RoomData.GetRooms().IndexOf(ActiveRoom.RoomName) - 1)]);
         }
         catch (Exception e)
         {
            Console.WriteLine(Global.GetExceptionMessage("The previous room does not exists.", e));
         }
      }

      public void Run()
      {
         Initialization(windowCaption);
         GameStart();
         MainLoop();
         GameEnd();
      }
      public void SetWindow()
      {
         IsFullScreen = true;
         SetWindow_local(new RenderWindow(VideoMode.DesktopMode, windowCaption, Styles.Fullscreen));
      }
      public void SetWindow(uint width, uint height)
      {
         IsFullScreen = false;
         SetWindow_local(new RenderWindow(new VideoMode(width, height), windowCaption, Styles.Close));
      }
      private void SetWindow_local(RenderWindow renderWindow)
      {
         if (Window != null)
            Window.Dispose();

         Window = renderWindow;
         Window.SetVisible(true);
         Window.SetVerticalSyncEnabled(true);
         Window.SetKeyRepeatEnabled(false);

         Window.Closed += new EventHandler(WindowClosed);
         Window.KeyPressed += new EventHandler<KeyEventArgs>(KeyPressed);
         Window.KeyReleased += new EventHandler<KeyEventArgs>(KeyReleased);

         blackScreen.Size = (Vector2f)Window.Size;
      }

      // Phases
      private void Initialization(string windowCaption)
      {
         SetWindow(1920, 1080);
         Clock.Restart();
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

            roomChanged = false;
            foreach (GameObject gameObject in ActiveRoom.Instances_SortedByCreation.Values)
            {
               gameObject.Loop();
               if (roomChanged)
                  break;
            }

            pressKeyboard.Clear();
            releaseKeyboard.Clear();
            if (roomChanged)
               continue;

            Window.Clear(BACKGROUND_COLOR);
            Draw();

            Window.DispatchEvents();
            Window.Display();
            
         }
      }
      private void Draw()
      {
         foreach (GameObject gameObject in ActiveRoom.Instances_SortedByDepth)
            gameObject.Draw();

         if (blackScreen.FillColor.A > 0)
         {
            int newAlpha = (int)(blackScreen.FillColor.A * Math.Pow(0.1, DeltaTime / 1000d));
            if (newAlpha > 0)
            {
               Window.Draw(blackScreen);
               blackScreen.FillColor = new Color(0, 0, 0, (byte)newAlpha);
            }
            else
            {
               Window.Draw(blackScreen);
               blackScreen.FillColor = new Color(0, 0, 0, 0);
            }
         }
      }
      private void GameEnd()
      {
      }

      // Events
      private void WindowClosed(object sender, EventArgs e)
      {
         GameEnd();
         Window.Close();
      }
      private void KeyPressed(object sender, KeyEventArgs e)
      {
         checkKeyboard.TryAdd(e.Code, true);
         pressKeyboard.TryAdd(e.Code, true);
      }
      private void KeyReleased(object sender, KeyEventArgs e)
      {
         releaseKeyboard.TryAdd(e.Code, true);
         checkKeyboard.Remove(e.Code);
      }

      public bool CheckKey(Keyboard.Key key)
      {
         return checkKeyboard.GetValueOrDefault(key, false);
      }
      public bool IsKeyPressed(Keyboard.Key key)
      {
         return pressKeyboard.GetValueOrDefault(key, false);
      }
      public bool IsKeyReleased(Keyboard.Key key)
      {
         return releaseKeyboard.GetValueOrDefault(key, false);
      }

      public int DeltaTime { get; private set; }
      public RenderWindow Window { get; set; }
      public RoomData RoomData { get; set; }
      public bool IsFullScreen { get; private set; }
      public Clock Clock { get; }
      public static Game ActiveGame { get; internal set; }
      public Room ActiveRoom { get; private set; }
   }
}