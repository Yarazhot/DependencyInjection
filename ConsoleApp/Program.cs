using System;
using System.Collections.Generic;
using DILib;
using NLog;
using NLog.Config;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            LogManager.Configuration = new XmlLoggingConfiguration("NLog.config");
            var logger = LogManager.GetCurrentClassLogger();

            try
            {
                var config = new DiConfig();
                config.AddGenerator<Son>(new Fabric(() => new Son("Vania")));
                config.AddGenerator<Daughter>(new Fabric(() => new Daughter("Liza")));
                config.AddFabricGenerator<Parent>();

                var provider = new DiProvider(config);

                var parent = provider.Inject<Parent>();
                
                config.AddSingleGenerator<Parent>();
            }
            catch (Exception e)
            {
                logger.Error(e.ToString);
            }
        }

        class Parent
        {
            private IEnumerable<Child> children;

            public Parent(IEnumerable<Child> children)
            {
                this.children = children;
            }
        }

        abstract class Child
        {
            public String name { get; set; }
        }

        class Son : Child
        {
            public Son(String name)
            {
                this.name = name;
            }
        }

        class Daughter : Child
        {
            public Daughter(String name)
            {
                this.name = name;
            }
        }
    }
}