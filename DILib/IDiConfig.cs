using System;

namespace DILib
{
    public interface IDiConfig
    {
        public object Get(Type type);

        public void AddGenerator<T>(IGenerator generator);

        public void AddSingleGenerator<T>();
        public void AddSingleGenerator<TFor, T>();

        public void AddFabricGenerator<T>();
        public void AddFabricGenerator<TFor, T>();
        public void Attach<T, TDefinition>();
        public void AttachHard<T, TDefinition>();
    }
}