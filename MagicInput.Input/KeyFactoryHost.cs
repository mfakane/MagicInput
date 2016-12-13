using System;
using System.Collections.Generic;
using System.Composition;
using System.Composition.Convention;
using System.Composition.Hosting;
using System.Reflection;

namespace MagicInput.Input
{
	public class KeyFactoryHost : IDisposable
	{
		readonly CompositionHost host;

		[ImportMany]
		public IList<IKeyProvider> Providers { get; set; }

		public KeyFactoryHost(IEnumerable<Assembly> assemblies)
		{
			var conventions = new ConventionBuilder();
			
			conventions
				.ForTypesDerivedFrom<IKeyProvider>()
				.Export<IKeyProvider>();
			
			host = new ContainerConfiguration()
				.WithAssemblies(assemblies)
				.WithDefaultConventions(conventions)
				.CreateContainer();
			host.SatisfyImports(this);
		}

		public void Dispose() =>
			host.Dispose();
	}
}
