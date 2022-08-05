using FluentAssertions;
using LanguageExt.TypeClasses;

namespace PatchExample;

// some data
public sealed record Customer(string Name, int Age, DateTime Dob);

// equality provider for the data
public readonly struct EqCustomer : Eq<Customer>
{
    public Task<int> GetHashCodeAsync(Customer x) =>
        x.GetHashCode().AsTask();

    public int GetHashCode(Customer x) =>
        x.GetHashCode();

    public Task<bool> EqualsAsync(Customer x, Customer y) =>
        Equals(x, y).AsTask();

    public bool Equals(Customer x, Customer y) =>
        x == y;
}

public static class DiffTests
{
    [Fact]
    public static void DiffTest()
    {
        // list of customers to set
        var newCustomers = Seq(
            new Customer(
                "Ben",
                35,
                new DateTime(1896, 12, 5).Date), // watch out for equals on dates, need to truncate them correctly
            new Customer("John", 34, new DateTime(1987, 12, 4).Date),
            new Customer("Mike", 37, new DateTime(1984, 12, 7).Date),
            new Customer("Steve", 25, new DateTime(1996, 12, 12).Date));

        // get an existing list from the api
        var currentCustomers = Seq(
            new Customer("Ben", 35, new DateTime(1896, 12, 5).Date),
            new Customer("James", 34, new DateTime(1987, 12, 7).Date),
            new Customer("Mike", 25, new DateTime(1996, 12, 2).Date));

        // get a patch of the difference
        var differences = Patch.diff<EqCustomer, Customer>(currentCustomers, newCustomers);
        differences.Edits.Should()
            .NotBeEmpty()
            .And
            .Equal(
                // the path required to make them the same
                // leave the first element unchanged, replace the next 2 elements and then insert a new element at the end 
                new Edit<EqCustomer, Customer>.Replace(
                    1,
                    new Customer("James", 34, new DateTime(1987, 12, 7).Date),
                    new Customer("John", 34, new DateTime(1987, 12, 4).Date)),
                new Edit<EqCustomer, Customer>.Replace(
                    2,
                    new Customer("Mike", 25, new DateTime(1996, 12, 2).Date),
                    new Customer("Mike", 37, new DateTime(1984, 12, 7).Date)),
                new Edit<EqCustomer, Customer>.Insert(3, new Customer("Steve", 25, new DateTime(1996, 12, 12).Date)));

        // now iterate over the patches and do something with them, ie call and API for database
    }
}