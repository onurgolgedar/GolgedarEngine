namespace GolgedarEngine
{
    public abstract class ConnectedGameObject : GameObject
    {
        int connectionID;

        public ConnectedGameObject(int connectionID, string imageName = "") : base(imageName)
        {
            this.connectionID = connectionID;
        }

        public int ConnectionID { get => connectionID; set => connectionID = value; }
    }
}