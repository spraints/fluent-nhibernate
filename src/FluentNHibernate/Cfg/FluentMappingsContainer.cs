using System;
using System.Collections.Generic;
using System.Reflection;
using FluentNHibernate.Mapping;
using FluentNHibernate.Visitors;
using NHibernate.Cfg;
using System.IO;

namespace FluentNHibernate.Cfg
{
    /// <summary>
    /// Container for fluent mappings
    /// </summary>
    public class FluentMappingsContainer
    {
        private readonly IList<Assembly> assemblies = new List<Assembly>();
        protected List<Type> types = new List<Type>();
        private string exportPath;
        private TextWriter exportTextWriter;
        private readonly PersistenceModel model;

        internal FluentMappingsContainer()
        {
            model = new PersistenceModel();
        }

        public PersistenceModel PersistenceModel
        {
            get { return model; }
        }

        public FluentMappingsContainer OverrideBiDirectionalManyToManyPairing(PairBiDirectionalManyToManySidesDelegate userControlledPairing)
        {
            model.BiDirectionalManyToManyPairer = userControlledPairing;
            return this;
        }

        /// <summary>
        /// Add all fluent mappings in the assembly that contains T.
        /// </summary>
        /// <typeparam name="T">Type from the assembly</typeparam>
        /// <returns>Fluent mappings configuration</returns>
        public FluentMappingsContainer AddFromAssemblyOf<T>()
        {
            return AddFromAssembly(typeof(T).Assembly);
        }

        /// <summary>
        /// Add all fluent mappings in the assembly
        /// </summary>
        /// <param name="assembly">Assembly to add mappings from</param>
        /// <returns>Fluent mappings configuration</returns>
        public FluentMappingsContainer AddFromAssembly(Assembly assembly)
        {
            assemblies.Add(assembly);
            WasUsed = true;
            return this;
		}

        /// <summary>
        /// Adds a single <see cref="IClassMap" /> represented by the specified type.
        /// </summary>
        /// <returns>Fluent mappings configuration</returns>
        public FluentMappingsContainer Add<T>()
        {
            return Add(typeof(T));
        }

        /// <summary>
        /// Adds a single <see cref="IClassMap" /> represented by the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>Fluent mappings configuration</returns>
        public FluentMappingsContainer Add(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            types.Add(type);
            WasUsed = true;
            return this;
        }

		/// <summary>
        /// Sets the export location for generated mappings
        /// </summary>
        /// <param name="path">Path to folder for mappings</param>
        /// <returns>Fluent mappings configuration</returns>
        public FluentMappingsContainer ExportTo(string path)
        {
            exportPath = path;
            return this;
        }

        /// <summary>
        /// Sets the text writer to write the generated mappings to.
        /// </summary>                
        /// <returns>Fluent mappings configuration</returns>
        public FluentMappingsContainer ExportTo(TextWriter textWriter)
        {
            exportTextWriter = textWriter;
            return this;
        }

        /// <summary>
        /// Alter convention discovery
        /// </summary>
        public SetupConventionFinder<FluentMappingsContainer> Conventions
        {
            get { return new SetupConventionFinder<FluentMappingsContainer>(this, model.Conventions); }
        }

        /// <summary>
        /// Gets whether any mappings were added
        /// </summary>
        internal bool WasUsed { get; set; }

        /// <summary>
        /// Applies any added mappings to the NHibernate Configuration
        /// </summary>
        /// <param name="cfg">NHibernate Configuration instance</param>
        internal void Apply(Configuration cfg)
        {
            foreach (var assembly in assemblies)
            {
                model.AddMappingsFromAssembly(assembly);
            }

            foreach (var type in types)
            {
                model.Add(type);
            }

            if (!string.IsNullOrEmpty(exportPath))
                model.WriteMappingsTo(exportPath);

            if (exportTextWriter != null)
                model.WriteMappingsTo(exportTextWriter);

            model.Configure(cfg);
        }

        /// <summary>
        /// Accepts a custom construction mechanism for mapping provider types.
        /// </summary>
        public void ConstructBy(Func<Type, object> builder)
        {
            model.CreateInstanceFunc = builder;
        }
    }
}