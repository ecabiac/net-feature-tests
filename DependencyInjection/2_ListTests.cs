﻿using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Xunit;
using FeatureTests.Shared;
using FeatureTests.On.DependencyInjection.Adapters;
using FeatureTests.On.DependencyInjection.Adapters.Interface;
using FeatureTests.On.DependencyInjection.TestTypes;

namespace FeatureTests.On.DependencyInjection {
    [DisplayOrder(2)]
    [DisplayName("List/array dependencies")]
    [Description(@"
        When several registrations of `IService` exist in a container, many
        frameworks automatically provide `IService[]` (or `List<IService>` etc).

        This is extremely important for open/closed principle. For example,
        UserValidator can have dependency on `IUserValidationRule[]`. Then new
        rules can be transparently added to the system without any changes to
        UserValidator.
    ")]
    [FeatureScoring(FeatureScoring.SinglePoint)]
    public class ListTests {
        [Feature]
        [DisplayName("IService[]")]
        [SpecialCase(typeof(UnityAdapter), "Note: Unity requires named registrations for list resolution to work.")]
        public void Array(IContainerAdapter adapter) {
            this.AssertResolvesListDependencyFor<ServiceWithListConstructorDependency<IService[]>>(adapter);
        }

        [Feature]
        [DisplayName("IList<IService>")]
        public void List(IContainerAdapter adapter) {
            this.AssertResolvesListDependencyFor<ServiceWithListConstructorDependency<IList<IService>>>(adapter);
        }

        [Feature]
        [DisplayName("ICollection<IService>")]
        public void Collection(IContainerAdapter adapter) {
            this.AssertResolvesListDependencyFor<ServiceWithListConstructorDependency<ICollection<IService>>>(adapter);
        }

        [Feature]
        [DisplayName("IEnumerable<IService>")]
        public void Enumerable(IContainerAdapter adapter) {
            this.AssertResolvesListDependencyFor<ServiceWithListConstructorDependency<IEnumerable<IService>>>(adapter);
        }

        [Feature]
        [DisplayName("IReadOnlyCollection<IService>")]
        public void IReadOnlyCollection(IContainerAdapter adapter) {
            this.AssertResolvesListDependencyFor<ServiceWithListConstructorDependency<IReadOnlyCollection<IService>>>(adapter);
        }

        [Feature]
        [DisplayName("IReadOnlyList<IService>")]
        public void IReadOnlyList(IContainerAdapter adapter) {
            this.AssertResolvesListDependencyFor<ServiceWithListConstructorDependency<IReadOnlyList<IService>>>(adapter);
        }

        public void AssertResolvesListDependencyFor<TTestComponent>(IContainerAdapter adapter)
            where TTestComponent : IServiceWithListDependency<IEnumerable<IService>>
        {
            adapter.RegisterType<IService, IndependentService>();
            adapter.RegisterType<TTestComponent>();

            var resolved = adapter.Resolve<TTestComponent>();

            Assert.NotNull(resolved);
            Assert.NotNull(resolved.Services);
            Assert.Equal(1, resolved.Services.Count());
            Assert.IsAssignableFrom<IndependentService>(resolved.Services.First());
        }
    }
}
