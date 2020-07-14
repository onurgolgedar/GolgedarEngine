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
         return degrees * (float) (Math.PI / 180);
      }
      public static float ToDegrees(float radians)
      {
         return radians * 180 / (float) Math.PI;
      }
      public static void HandleNvidiaProfile(string applicationName)
      {
         try
         {
            NVIDIA.Initialize();

            DriverSettingsSession session = DriverSettingsSession.CreateAndLoad();

            DriverSettingsProfile profile = DriverSettingsProfile.CreateProfile(session, "Golgedar Engine");
            ProfileApplication.CreateApplication(profile, "Golgedar Engine");

            profile.SetSetting(KnownSettingId.OpenGLThreadControl, 2);

            session.Save();
            session.Dispose();
         }
         catch (Exception)
         {
         }
      }
   }
}
