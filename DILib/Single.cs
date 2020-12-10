namespace DILib
{
    public class Single : IGenerator
    {
        private readonly Create _create;
        private object _instance;

        public object Generate()
        {
            if (_instance == null)
            {
                lock (_create)
                {
                    _instance ??= _create();
                }
            }

            return _instance;
        }

        public Single(Create create)
        {
            _create = create;
        }
    }
}