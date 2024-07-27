using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System;
using System.Collections.Generic;
using System.Diagnostics;

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
                Debug.WriteLine(Global.GetExceptionMessage("The next room does not exist.", e));
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
                Debug.WriteLine(Global.GetExceptionMessage("The previous room does not exist.", e));
            }
        }

        public void Run()
        {
            Initialization(windowCaption);
            GameStart();
            ActiveRoom.Load();
            roomChanged = false;
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
            //Window.SetFramerateLimit(60);
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

                List<GameObject> collisionInstances = new List<GameObject>(ActiveRoom.InstancesWithCollisionMask_SortedByCreation);

                for (int gameObjectIndex = 0; gameObjectIndex < ActiveRoom.Instances_SortedByCreation.Count; gameObjectIndex++)
                {
                    GameObject gameObject = ActiveRoom.Instances_SortedByCreation.Values[gameObjectIndex];
                    gameObject.Loop();

                    int collisionIndex = collisionInstances.LastIndexOf(gameObject);
                    if (collisionIndex != -1)
                    {
                        collisionInstances.RemoveAt(collisionIndex);

                        foreach (GameObject collisionPair in collisionInstances)
                        {
                            if (gameObject.CollisionMask.GetGlobalBounds().Intersects(collisionPair.CollisionMask.GetGlobalBounds(), out FloatRect overlap))
                                gameObject.Collision(collisionPair);
                        }

                        collisionInstances.Insert(collisionIndex, gameObject);
                    }

                    if (roomChanged)
                    {
                        ActiveRoom.Load();
                        roomChanged = false;
                    }
                }

                Window.Clear(BACKGROUND_COLOR);
                Draw();

                pressKeyboard.Clear();
                releaseKeyboard.Clear();

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

/*
Vector2f myMaskCenter = gameObject.CollisionMask.Position + gameObject.CollisionMask.Size / 2;
Vector2f otherMaskCenter = secondaryGameObject.CollisionMask.Position + secondaryGameObject.CollisionMask.Size / 2;
Vector2f maskCenterDiff = myMaskCenter - otherMaskCenter;
float nonZeroComponent = maskCenterDiff.X != 0 ? maskCenterDiff.X : maskCenterDiff.Y;
int nonZeroComponentSign = Math.Sign(nonZeroComponent);

Vector2f totalDisplacement;

int oneDirection = Math.Abs(maskCenterDiff.X / maskCenterDiff.Y) > 4 ? 0 : 1;
if (Math.Sign(maskCenterDiff.X) == Math.Sign(maskCenterDiff.Y))
{
   if (overlap.Width < overlap.Height)
   {
      totalDisplacement = new Vector2f(overlap.Width, oneDirection * overlap.Width * (overlap.Width / overlap.Height)) * -nonZeroComponentSign;
      totalDisplacement.X = Math.Sign(totalDisplacement.X) * (float)Math.Ceiling(Math.Abs(totalDisplacement.X));
   }
   else
   {
      totalDisplacement = new Vector2f(oneDirection * overlap.Height * (overlap.Height / overlap.Width), overlap.Height) * -nonZeroComponentSign;
      totalDisplacement.Y = Math.Sign(totalDisplacement.Y) * (float)Math.Ceiling(Math.Abs(totalDisplacement.Y));
   }
}
else
{
   if (overlap.Width < overlap.Height)
   {
      totalDisplacement = new Vector2f(overlap.Width, oneDirection * -overlap.Width * (overlap.Width / overlap.Height)) * -nonZeroComponentSign;
      totalDisplacement.X = Math.Sign(totalDisplacement.X) * (float)Math.Ceiling(Math.Abs(totalDisplacement.X));
   }
   else
   {
      totalDisplacement = new Vector2f(oneDirection * overlap.Height * (overlap.Height / overlap.Width), -overlap.Height) * -nonZeroComponentSign;
      totalDisplacement.Y = Math.Sign(totalDisplacement.Y) * (float)Math.Ceiling(Math.Abs(totalDisplacement.Y));
   }
}

if (otherPusher.Weight > pusher.Weight)
{
   if (otherPusher.Weight < uint.MaxValue / 2)
   {
      Vector2f displacementDone = totalDisplacement / ((ulong)pusher.Weight + otherPusher.Weight) * pusher.Weight;
      secondaryGameObject.Position += displacementDone;
      totalDisplacement -= displacementDone;
   }

   if (pusher.Weight < uint.MaxValue / 2)
      gameObject.Position -= totalDisplacement;
}
else
{
   if (pusher.Weight < uint.MaxValue / 2)
   {
      Vector2f displacementDone = totalDisplacement / ((ulong)pusher.Weight + otherPusher.Weight) * otherPusher.Weight;
      gameObject.Position -= displacementDone;
      totalDisplacement -= displacementDone;
   }

   if (otherPusher.Weight < uint.MaxValue / 2)
      secondaryGameObject.Position += totalDisplacement;
}

queue.Enqueue(secondaryGameObject);
*/
