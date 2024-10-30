using FluentAssertions;

using NSubstitute;

using System;

using Xunit;

namespace LegacyApp.Tests.Unit;

public class UserServiceTests
{
    private readonly UserService _sut;

    private readonly IClock _clock = Substitute.For<IClock>();
    private readonly IClientRepository _clientRepository = Substitute.For<IClientRepository>();
    private readonly IUserCreditService _userCreditService = Substitute.For<IUserCreditService>();
    private readonly IUserDataAccessWrapper _userDataAccessWrapper = Substitute.For<IUserDataAccessWrapper>();

    public UserServiceTests()
    {
        var userCreditServiceFactory = Substitute.For<IUserCreditServiceFactory>();
        userCreditServiceFactory.Create().Returns(_userCreditService);

        _sut = new UserService(_clock, _clientRepository, userCreditServiceFactory, _userDataAccessWrapper);
    }

    [Fact]
    public void AddUser_ShouldNotCreateUser_WhenFirstNameIsEmpty()
    {
        string? firstName = null;
        string surname = "Wyffels";
        string email = "waut.wyffels@exsertus.be";
        DateTime dateOfBirth = new DateTime(1999, 12, 09);
        int clientId = 1;

        var result = _sut.AddUser(firstName, surname, email, dateOfBirth, clientId);

        result.Should().BeFalse();
    }

    [Fact]
    public void AddUser_ShouldNotCreateUser_WhenLastNameIsEmpty()
    {
        string firstName = "Waut";
        string? surname = null;
        string email = "waut.wyffels@exsertus.be";
        DateTime dateOfBirth = new DateTime(1999, 12, 09);
        int clientId = 1;

        var result = _sut.AddUser(firstName, surname, email, dateOfBirth, clientId);

        result.Should().BeFalse();
    }

    [Fact]
    public void AddUser_ShouldNotCreateUser_WhenEmailIsInvalid()
    {
        string firstName = "Waut";
        string surname = "Wyffels";
        string email = "thisemailiswrong";
        DateTime dateOfBirth = new DateTime(1999, 12, 09);
        int clientId = 1;

        var result = _sut.AddUser(firstName, surname, email, dateOfBirth, clientId);

        result.Should().BeFalse();
    }

    [Fact]
    public void AddUser_ShouldNotCreateUser_WhenUserIsUnder21()
    {
        string firstName = "Waut";
        string surname = "Wyffels";
        string email = "waut.wyffels@exsertus.be";
        DateTime dateOfBirth = new DateTime(1999, 12, 09);
        int clientId = 1;

        _clock.Now.Returns(new DateTime(2000, 01, 01));

        var result = _sut.AddUser(firstName, surname, email, dateOfBirth, clientId);

        result.Should().BeFalse();
    }

    [Fact]
    public void AddUser_ShouldNotCreateUser_WhenUserHasCreditLimitAndLimitIsLessThan500()
    {
        string firstName = "Waut";
        string surname = "Wyffels";
        string email = "waut.wyffels@exsertus.be";
        DateTime dateOfBirth = new DateTime(1999, 12, 09);
        int clientId = 1;

        _clock.Now.Returns(new DateTime(2023, 01, 01));

        _clientRepository.GetById(clientId)
            .Returns(new Client
            {
                Id = clientId,
                Name = "Euronav",
                ClientStatus = ClientStatus.Titanium
            });

        _userCreditService.GetCreditLimit(firstName, surname, dateOfBirth)
            .Returns(100);

        var result = _sut.AddUser(firstName, surname, email, dateOfBirth, clientId);

        result.Should().BeFalse();
    }

    [Fact]
    public void AddUser_ShouldCreateUser_WhenUserDetailsAreValid()
    {
        string firstName = "Waut";
        string surname = "Wyffels";
        string email = "waut.wyffels@exsertus.be";
        DateTime dateOfBirth = new DateTime(1999, 12, 09);
        int clientId = 1;

        _clock.Now.Returns(new DateTime(2023, 01, 01));

        _clientRepository.GetById(clientId)
            .Returns(new Client
            {
                Id = clientId,
                Name = "Euronav",
                ClientStatus = ClientStatus.Titanium
            });

        _userCreditService.GetCreditLimit(firstName, surname, dateOfBirth)
            .Returns(1000);

        var result = _sut.AddUser(firstName, surname, email, dateOfBirth, clientId);

        result.Should().BeTrue();
    }
}
