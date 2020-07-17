using NvAPIWrapper;
using NvAPIWrapper.DRS;
using SFML.Graphics;
using SFML.System;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;

namespace GolgedarEngine
{
   public static class Global
   {
      private static readonly Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();

      private static void AddSprite(string imageName, Sprite sprite)
      {
         sprites.Add(imageName, sprite);
      }
      internal static Sprite CreateSprite(string imageName)
      {
         string path = Path.Combine(Environment.CurrentDirectory, @".Sources\.Images\" + imageName);

         Image image = new Image(path);
         Texture texture = new Texture(image);
         Sprite sprite = new Sprite(texture);

         return sprite;
      }
      public static Sprite GetSprite(string imageName)
      {
         return sprites.GetValueOrDefault(imageName, null);
      }
      public static void LoadSprites(ImmutableList<string> imageNames)
      {
         foreach (string imageName in imageNames)
         {
            Sprite sprite = CreateSprite(imageName);
            sprite.Origin = new Vector2f(sprite.Texture.Size.X / 2, sprite.Texture.Size.X / 2);
            AddSprite(imageName, sprite);
         }
      }
      public static float ToRad(float degrees)
      {
         return degrees * (float)(Math.PI / 180);
      }
      public static float ToDegrees(float radians)
      {
         return radians * 180 / (float)Math.PI;
      }
      public static void HandleNvidiaProfile(string profileName, string fileName)
      {
         try
         {
            NVIDIA.Initialize();
         }
         catch (Exception e)
         {
            Console.WriteLine(GetExceptionMessage("NVIDIA Profile could not be set.", e));
         }

         DriverSettingsSession session = DriverSettingsSession.CreateAndLoad();
         DriverSettingsProfile profile = null;

         foreach (DriverSettingsProfile _profile in session.Profiles)
            if (_profile.Name == profileName)
               profile = _profile;

         if (profile == null)
         {
            profile = DriverSettingsProfile.CreateProfile(session, profileName);
            Console.WriteLine("Profile[" + profile.Name + "] is created.");
         }

         profile.SetSetting(KnownSettingId.OpenGLThreadControl, 2);

         ProfileApplication profileApplication = session.FindApplication(fileName);

         if (profileApplication == null)
         {
            ProfileApplication.CreateApplication(profile, fileName);
            Console.WriteLine("ApplicationIndex[" + fileName + "] is created for Profile[" + profile.Name + "].");
         }
         else if (profileApplication.Profile.Name != profile.Name)
         {
            profileApplication.Delete();
            ProfileApplication.CreateApplication(profile, fileName);
            Console.WriteLine("ApplicationIndex[" + fileName + "] is attached to Profile[" + profile.Name + "].");
         }

         session.Save();
         session.Dispose();
      }
      public static string GetExceptionMessage(string description, Exception exception)
      {
         return "[ERROR MESSAGE]: " + description + "\nException Message: " + exception.Message + "\n" + exception.StackTrace + "\n[MESSAGE END]";
      }
   }
}
