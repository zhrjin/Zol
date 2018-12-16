using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Dependencies;
using Ninject;
using Ninject.Parameters;
using Ninject.Syntax;

namespace Zol.OrleansWebApi.IoC
{
    public class NinjectResolver : NinjectScope, IDependencyResolver
    {
        private readonly IKernel kernel;

        public NinjectResolver(IKernel kernel)
            : base(kernel)
        {
            this.kernel = kernel;
        }

        public IDependencyScope BeginScope()
        {
            return new NinjectScope(this.kernel.BeginBlock());
        }
    }

    public class NinjectScope : IDependencyScope
    {
        protected IResolutionRoot ResolutionRoot;
        public NinjectScope(IResolutionRoot kernel)
        {
            this.ResolutionRoot = kernel;
        }

        public void Dispose()
        {
            var disposable = (IDisposable)this.ResolutionRoot;
            disposable?.Dispose();
            this.ResolutionRoot = null;
        }

        public object GetService(Type serviceType)
        {
            var request = this.ResolutionRoot.CreateRequest(serviceType, null, new Parameter[0], true, true);
            return this.ResolutionRoot.Resolve(request).SingleOrDefault();
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            var request = this.ResolutionRoot.CreateRequest(serviceType, null, new Parameter[0], true, true);
            return this.ResolutionRoot.Resolve(request).ToList();
        }
    }
}