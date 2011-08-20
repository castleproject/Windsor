using NUnit.Framework;

namespace Castle.Transactions.Tests
{
	public class MaybeMonad_Specs
	{
		[Test]
		public void WhenHas_SomeValue()
		{
			var m = Maybe.Some(5);
			Assert.That(m.HasValue);
			Assert.That(m.Value, Is.EqualTo(5));
		}

		[Test]
		public void Maybe_GetValue_Throws_IfNoValue()
		{
			var m = Maybe.None<int>();
			Assert.That(m.HasValue, Is.False);
			Assert.That(() => m.Value, Throws.InvalidOperationException);
		}

		[Test]
		public void Amb_Returns_First_WithValue()
		{
			var m = Maybe.None<int>();
			Assert.That(m.Amb(() => Maybe.Some(4)).HasValue);
			Assert.That(m.Amb(Maybe.None<int>).HasValue, Is.False);

			var n = Maybe.Some(10);
			Assert.That(n.Amb(Maybe.None<int>).HasValue);


			Maybe<int> amb = n.Amb(() => Maybe.Some(4));
			Assert.That(amb.HasValue);
			Assert.That(amb.Value, Is.EqualTo(10));
		}
	}
}