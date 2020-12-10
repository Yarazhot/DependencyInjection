namespace DILib
{
    public class Fabric : IGenerator
    {
        private readonly Create _create;

        public object Generate()
        {
            return _create();
        }

        public Fabric(Create create)
        {
            _create = create;
        }
    }
}