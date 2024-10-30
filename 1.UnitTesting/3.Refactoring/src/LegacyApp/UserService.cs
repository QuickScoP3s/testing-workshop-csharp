using System;

namespace LegacyApp
{
    public class UserService
    {
        private readonly IClock _clock;
        private readonly IClientRepository _clientRepository;
        private readonly IUserCreditServiceFactory _userCreditServiceFactory;
        private readonly IUserDataAccessWrapper _userDataAccessWrapper;

        public UserService()
        {
            this._clock = new Clock();
            this._clientRepository = new ClientRepository();
            this._userCreditServiceFactory = new UserCreditServiceFactory();
            this._userDataAccessWrapper = new UserDataAccessWrapper();
        }

        internal UserService(IClock clock, IClientRepository clientRepository, IUserCreditServiceFactory userCreditServiceFactory, IUserDataAccessWrapper userDataAccessWrapper)
        {
            this._clock = clock;
            this._clientRepository = clientRepository;
            this._userCreditServiceFactory = userCreditServiceFactory;
            this._userDataAccessWrapper = userDataAccessWrapper;
        }

        public bool AddUser(string firname, string surname, string email, DateTime dateOfBirth, int clientId)
        {
            if (string.IsNullOrEmpty(firname) || string.IsNullOrEmpty(surname))
            {
                return false;
            }

            if (!email.Contains("@") && !email.Contains("."))
            {
                return false;
            }

            var now = _clock.Now;
            int age = now.Year - dateOfBirth.Year;
            if (now.Month < dateOfBirth.Month || (now.Month == dateOfBirth.Month && now.Day < dateOfBirth.Day)) age--;

            if (age < 21)
            {
                return false;
            }

            var client = _clientRepository.GetById(clientId);

            var user = new User
            {
                Client = client,
                DateOfBirth = dateOfBirth,
                EmailAddress = email,
                Firstname = firname,
                Surname = surname
            };

            if (client.Name == "VeryImportantClient")
            {
                // Skip credit check
                user.HasCreditLimit = false;
            }
            else if (client.Name == "ImportantClient")
            {
                // Do credit check and double credit limit
                user.HasCreditLimit = true;

                using (var userCreditService = _userCreditServiceFactory.Create())
                {
                    var creditLimit = userCreditService.GetCreditLimit(user.Firstname, user.Surname, user.DateOfBirth);

                    creditLimit = creditLimit * 2;
                    user.CreditLimit = creditLimit;
                }
            }
            else
            {
                // Do credit check
                user.HasCreditLimit = true;

                using (var userCreditService = _userCreditServiceFactory.Create())
                {
                    var creditLimit = userCreditService.GetCreditLimit(user.Firstname, user.Surname, user.DateOfBirth);
                    user.CreditLimit = creditLimit;
                }
            }

            if (user.HasCreditLimit && user.CreditLimit < 500)
            {
                return false;
            }

            _userDataAccessWrapper.AddUser(user);
            return true;
        }
    }
}
