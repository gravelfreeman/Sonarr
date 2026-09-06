using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.RootFolders;
using NzbDrone.Core.Validation;
using NzbDrone.Core.Validation.Paths;
using Sonarr.Http;

namespace Sonarr.Api.V3.Config
{
    [V3ApiController("config/mediamanagement")]
    public class MediaManagementConfigController : ConfigController<MediaManagementConfigResource>
    {
        private readonly IRootFolderService _rootFolderService;
        private readonly IRootFolderRepository _rootFolderRepository;
        private readonly IEventAggregator _eventAggregator;

        public MediaManagementConfigController(IConfigService configService,
                                           IRootFolderService rootFolderService,
                                           IRootFolderRepository rootFolderRepository,
                                           IEventAggregator eventAggregator,
                                           FolderChmodValidator folderChmodValidator)
            : base(configService)
        {
            _rootFolderService = rootFolderService;
            _rootFolderRepository = rootFolderRepository;
            _eventAggregator = eventAggregator;

            SharedValidator.RuleFor(c => c.RecycleBinCleanupDays).GreaterThanOrEqualTo(0);
            SharedValidator.RuleFor(c => c.ChmodFolder).SetValidator(folderChmodValidator).When(c => !string.IsNullOrEmpty(c.ChmodFolder) && (OsInfo.IsLinux || OsInfo.IsOsx));

            SharedValidator.RuleFor(c => c.ScriptImportPath).IsValidPath().When(c => c.UseScriptImport);

            SharedValidator.RuleFor(c => c.MinimumFreeSpaceWhenImporting).GreaterThanOrEqualTo(100);
            SharedValidator.RuleForEach(c => c.RootFolderUpdates).ChildRules(c => c.RuleFor(x => x.Id).GreaterThan(0));
            SharedValidator.RuleFor(c => c.RootFolderUpdates)
                .Must(c => c == null || (c.All(x => x != null) && c.Select(x => x.Id).Distinct().Count() == c.Count))
                .WithMessage("Root folder updates must contain valid, unique IDs");
        }

        public override ActionResult<MediaManagementConfigResource> SaveConfig([FromBody] MediaManagementConfigResource resource)
        {
            var updates = resource.RootFolderUpdates ?? new List<RootFolderUpdateResource>();
            var rootFolders = _rootFolderService.All().ToDictionary(x => x.Id);
            var failures = updates.Where(x => !rootFolders.ContainsKey(x.Id))
                                  .Select(x => new ValidationFailure("RootFolderUpdates", $"Root folder {x.Id} does not exist"))
                                  .ToList();

            if (failures.Any())
            {
                throw new ValidationException(failures);
            }

            var models = updates.Select(x =>
            {
                var rootFolder = rootFolders[x.Id];
                rootFolder.RecycleBinEnabled = x.RecycleBinEnabled;
                return rootFolder;
            }).ToList();

            var configValues = resource.GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(x => x.Name != nameof(MediaManagementConfigResource.RootFolderUpdates))
                .ToDictionary(prop => prop.Name, prop => prop.GetValue(resource, null));

            _configService.SaveConfigDictionary(configValues, (connection, transaction) =>
                _rootFolderRepository.UpdateRecycleBinEnabled(models, connection, transaction));

            foreach (var rootFolder in models)
            {
                _eventAggregator.PublishEvent(new ModelEvent<RootFolder>(rootFolder, ModelAction.Updated));
            }

            return Accepted(resource.Id);
        }

        protected override MediaManagementConfigResource ToResource(IConfigService model)
        {
            return MediaManagementConfigResourceMapper.ToResource(model);
        }
    }
}
