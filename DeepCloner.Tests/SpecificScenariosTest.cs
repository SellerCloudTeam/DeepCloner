using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;

using NUnit.Framework;

namespace Force.DeepCloner.Tests
{
	[TestFixture(true)]
	public class SpecificScenariosTest : BaseTest
	{
		public SpecificScenariosTest(bool isSafeInit)
			: base(isSafeInit)
		{
		}

		[Test]
		public void Test_ExpressionTree_OrderBy1()
		{
			var q = Enumerable.Range(1, 5).Reverse().AsQueryable().OrderBy(x => x);
			var q2 = q.DeepClone();
			Assert.That(q2.ToArray()[0], Is.EqualTo(1));
			Assert.That(q.ToArray().Length, Is.EqualTo(5));
		}

		[Test]
		public void Test_ExpressionTree_OrderBy2()
		{
			var l = new List<int> { 2, 1, 3, 4, 5 }.Select(y => new Tuple<int, string>(y, y.ToString(CultureInfo.InvariantCulture)));
			var q = l.AsQueryable().OrderBy(x => x.Item1);
			var q2 = q.DeepClone();
			Assert.That(q2.ToArray()[0].Item1, Is.EqualTo(1));
			Assert.That(q.ToArray().Length, Is.EqualTo(5));
		}

		[Test(Description = "Tests works on local SQL Server with AdventureWorks database")]
		[Ignore("Test on MS Server")]
		public void Clone_EfQuery1()
		{
			var at = new AdventureContext();
			// var at2 = at.DeepClone();
			// Console.WriteLine(at.ChangeTracker);
			// Console.WriteLine(at.ChangeTracker);
			var q = at.Currencies.Where(x => x.CurrencyCode == "AUD");
			var q2 = q.DeepClone();

			// Console.WriteLine(Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions
			//	.GetRequiredService<Microsoft.EntityFrameworkCore.ChangeTracking.Internal.IChangeTrackerFactory>(
			//		((Microsoft.EntityFrameworkCore.Infrastructure.IInfrastructure<IServiceProvider>) at).Instance));
			/* Console.WriteLine(((Microsoft.EntityFrameworkCore.Infrastructure.IInfrastructure<IServiceProvider>) at).Instance);
			var serviceProvider = ((Microsoft.EntityFrameworkCore.Infrastructure.IInfrastructure<IServiceProvider>) at.DeepClone()).Instance;
			Console.WriteLine(serviceProvider);
			Console.WriteLine(serviceProvider.GetService(typeof(Microsoft.EntityFrameworkCore.ChangeTracking.Internal.IChangeTrackerFactory)));
			Console.WriteLine(Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions
				.GetRequiredService<Microsoft.EntityFrameworkCore.ChangeTracking.Internal.IChangeTrackerFactory>(
					serviceProvider));*/

			// var q2 = q.DeepClone();
			// Console.WriteLine(q2.);
			// Assert.That(q.ToArray().Length, Is.EqualTo(1));
			Assert.That(q2.ToArray().Length, Is.EqualTo(1));
		}

		[Test(Description = "Tests works on local SQL Server with AdventureWorks database")]
		[Ignore("Test on MS Server")]
		public void Clone_EfQuery2()
		{
			var q = new AdventureContext().Currencies.OrderBy(x => x.Name);
			var q2 = q.DeepClone();
			var cnt = q.Count();
			Assert.That(q2.Count(), Is.EqualTo(cnt));
		}

		[Test]
		public void Lazy_Clone()
		{
			var lazy = new LazyClass();
			var clone = lazy.DeepClone();
			var v = LazyClass.Counter;
			Assert.That(clone.GetValue(), Is.EqualTo((v + 1).ToString(CultureInfo.InvariantCulture)));
			Assert.That(lazy.GetValue(), Is.EqualTo((v + 2).ToString(CultureInfo.InvariantCulture)));
		}

		public class LazyClass
		{
			public static int Counter;
			
			private readonly LazyRef<object> _lazyValue = new LazyRef<object>(() => (object)(++Counter).ToString(CultureInfo.InvariantCulture));

			public string GetValue()
			{
				return _lazyValue.Value.ToString();
			}
		}

		[Table("Currency", Schema = "Sales")]
		public class Currency
		{
			[Key]
			public string CurrencyCode { get; set; }

			[Column]
			public string Name { get; set; }
		}

		public class AdventureContext : DbContext
		{
			public AdventureContext()
			{
			}

			public DbSet<Currency> Currencies { get; set; }

#if NETCORE
			protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
			{
				optionsBuilder.UseSqlServer(@"Server=.;Database=AdventureWorks;Trusted_Connection=true;MultipleActiveResultSets=true");
			}
#endif
		}

		[Test]
		public void GenericComparer_Clone()
		{
			var comparer = new TestComparer();
			comparer.DeepClone();
		}

		[Test]
		public void Closure_Clone()
		{
			int a = 0;
			Func<int> f = () => ++a;
			var fCopy = f.DeepClone();
			Assert.That(f(), Is.EqualTo(1));
			Assert.That(fCopy(), Is.EqualTo(1));
			Assert.That(a, Is.EqualTo(1));
		}

		private class TestComparer : Comparer<int>
		{
			// make object unsafe to work
			private object _fieldX = new object();

			public override int Compare(int x, int y)
			{
				return x.CompareTo(y);
			}
		}

		public sealed class LazyRef<T>
		{
			private Func<T> _initializer;
			private T _value;

			/// <summary>
			///     This API supports the Entity Framework Core infrastructure and is not intended to be used
			///     directly from your code. This API may change or be removed in future releases.
			/// </summary>
			public T Value
			{
				get
				{
					if (_initializer != null)
					{
						_value = _initializer();
						_initializer = null;
					}
					return _value;
				}
				set
				{
					_value = value;
					_initializer = null;
				}
			}
			
			public LazyRef(Func<T> initializer)
			{
				_initializer = initializer;
			}
		}
	}
}
