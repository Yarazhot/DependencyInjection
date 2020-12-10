using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NLog;

namespace DILib
{
    public class DiConfig : IDiConfig
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly Dictionary<Type, IGenerator> Defined = new Dictionary<Type, IGenerator>();


        public object Get(Type type)
        {
            _logger.Trace("Get "+type.Name);
            if (typeof(IEnumerable).IsAssignableFrom(type))
            {
                _logger.Debug($"{type.Name} is IEnumerable");
                var genericArgument = type.GetGenericArguments()[0];
                try
                {
                    return GetAll(genericArgument);
                }
                catch (TypeNotDefinedException)
                {
                    
                }
            }

            //Check if needed type is defined
            if (Defined.ContainsKey(type))
            {
                _logger.Debug($"{type.Name} is Defined");
                return Defined[type].Generate();
            }

            //Search for inherited types
            foreach (var definedType in Defined.Keys)
            {
                if (type.IsAssignableFrom(definedType))
                {
                    _logger.Debug($"{type.Name} will be inject as {definedType.Name}");
                    return Defined[definedType].Generate();
                }
            }
            _logger.Error($"{type.Name} not defined at all");
            throw new TypeNotDefinedException();
        }

        private IEnumerable GetAll(Type type)
        {
            _logger.Trace("GetAll "+type.Name);
            var constructed = typeof(List<>).MakeGenericType(type);
            var list = (IList) Activator.CreateInstance(constructed);
            if (Defined.ContainsKey(type))
            {
                _logger.Debug($"find definition for {type.Name}");
                list.Add(Defined[type].Generate());
            }

            foreach (var definedType in Defined.Keys)
            {
                if (type.IsAssignableFrom(definedType))
                {
                    _logger.Debug($"find assignable definition for {type.Name} by {definedType.Name}");
                    list.Add(Defined[definedType].Generate());
                }
            }

            if (list.Count > 0)
            {
                return list;
            }

            else
            {
                _logger.Warn($"{type.Name} not defined at all");
                throw new TypeNotDefinedException();
            }
        }

        public void AddGenerator<T>(IGenerator generator)
        {
            lock (Defined)
            {
                if (!Defined.ContainsKey(typeof(T)))
                    Defined.Add(typeof(T), generator);
                else
                    throw new AlreadyDefinedException();
            }
        }

        public void AddSingleGenerator<T>()
        {
            var generator = new Single(GenerateCreateFromConstructor(GetConstructor<T>()));
            AddGenerator<T>(generator);
        }

        public void AddSingleGenerator<TFor, T>()
        {
            var generator = new Single(GenerateCreateFromConstructor(GetConstructor<T>()));
            AddGenerator<TFor>(generator);
            if (!Defined.ContainsKey(typeof(T)))
            {
                AddGenerator<T>(generator);
            }
        }

        public void AddFabricGenerator<T>()
        {
            var generator = new Fabric(GenerateCreateFromConstructor(GetConstructor<T>()));
            AddGenerator<T>(generator);
        }

        public void AddFabricGenerator<TFor, T>()
        {
            var generator = new Fabric(GenerateCreateFromConstructor(GetConstructor<T>()));
            AddGenerator<TFor>(generator);
            if (!Defined.ContainsKey(typeof(T)))
            {
                AddGenerator<T>(generator);
            }
        }

        public void Attach<T, TDefinition>()
        {
            if (!Defined.ContainsKey(typeof(TDefinition))) throw new TypeNotDefinedException();
            if (Defined.ContainsKey(typeof(T))) throw new AlreadyDefinedException();

            var definition = Defined[typeof(TDefinition)];
            Defined.Add(typeof(T), definition);
        }

        public void AttachHard<T, TDefinition>()
        {
            if (!Defined.ContainsKey(typeof(TDefinition))) throw new TypeNotDefinedException();

            var definition = Defined[typeof(TDefinition)];
            if (Defined.ContainsKey(typeof(T)))
            {
                Defined[typeof(T)] = definition;
            }
            else
            {
                Defined.Add(typeof(T), definition);
            }
        }

        private Create GenerateCreateFromConstructor(ConstructorInfo constructor)
        {
            return () =>
            {
                return constructor.Invoke(constructor
                    .GetParameters()
                    .Select(info => info.ParameterType)
                    .Select(parameter => Get(parameter))
                    .ToArray()
                );
            };
        }

        private static ConstructorInfo GetConstructor<T>()
        {
            var type = typeof(T);

            var constructors = type.GetConstructors();
            if (constructors.Length > 0)
            {
                return constructors[0];
            }

            else
            {
                throw new NotFitConstructorException();
            }
        }
    }
}