namespace DiCore.Lib.NDT.Types
{
    public struct CDpaDirection
    {
        public int Id { get; set; }
        public float Angle { get; set; }
        public float EntryAngle { get; set; }
        public enDirectionName DirectionName { get; set; }

        public CDpaDirection(int id, float angle) : this()
        {
            Id = id;
            Angle = angle;
        }
    }
}
