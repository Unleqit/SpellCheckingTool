using SpellCheckingTool.Application.Dictionary;
using SpellCheckingTool.Application.Persistence;
using SpellCheckingTool.Application.Users;
using SpellCheckingTool.Domain.Alphabet;
using SpellCheckingTool.Infrastructure.Dictionary;
using SpellCheckingTool.Infrastructure.FilePersistence;
using SpellCheckingTool.Infrastructure.UserPersistence;
using SpellCheckingTool.Infrastructure.UserSettingsPersistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpellCheckingTool.Presentation.Composition
{
    public class ApplicationFactory
    {
        private readonly IAlphabet _inputAlphabet = new UTF16Alphabet();

        private readonly string _basePath =
            Path.Combine(AppContext.BaseDirectory, "data");

        private readonly FileUserSettingsRepository _userSettingsRepository;
        private readonly FileUserStore _store;

        private readonly IDefaultDictionaryProvider _defaultDictionaryProvider;

        public ApplicationFactory()
        {
            _userSettingsRepository = new FileUserSettingsRepository(
                Path.Combine(_basePath, "UserSettings"));

            _store = new FileUserStore(
                _basePath,
                _inputAlphabet,
                _userSettingsRepository
            );

            var persistenceService = new FilePersistenceService();
            var dictionaryLoader = new DictionaryLoader(persistenceService);
            _defaultDictionaryProvider = new DefaultDictionaryLoader(dictionaryLoader);
        }
        public UserService CreateUserService()
        {
            return new UserService(_store, _store, _store);
        }

        public IUserSpellcheckContextFactory CreateSpellcheckFactory(UserService userService)
        {
            return new UserSpellcheckContextFactory(
                _defaultDictionaryProvider,
                userService,
                _userSettingsRepository,
                _inputAlphabet
            );
        }
    }
}
