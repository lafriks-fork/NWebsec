﻿// Copyright (c) André N. Klingsheim. See License.txt in the project root for license information.

using System;
using System.Security.Cryptography;
using NWebsec.Core.Common.HttpHeaders.Configuration;
using NWebsec.Core.Common.Web;
using NWebsec.Mvc.Common.Csp;

namespace NWebsec.Mvc.Common.Helpers
{
    public class CspConfigurationOverrideHelper : ICspConfigurationOverrideHelper
    {
        private readonly IContextConfigurationHelper _contextConfigurationHelper;
        private readonly ICspConfigMapper _configMapper;
        private readonly ICspDirectiveOverrideHelper _cspDirectiveOverrideHelper;

        public CspConfigurationOverrideHelper()
        {
            _cspDirectiveOverrideHelper = new CspDirectiveOverrideHelper();
            _contextConfigurationHelper = new ContextConfigurationHelper();
            _configMapper = new CspConfigMapper();
        }

        internal CspConfigurationOverrideHelper(IContextConfigurationHelper contextConfigurationHelper, ICspConfigMapper mapper, ICspDirectiveOverrideHelper directiveOverrideHelper)
        {
            _cspDirectiveOverrideHelper = directiveOverrideHelper;
            _contextConfigurationHelper = contextConfigurationHelper;
            _configMapper = mapper;
        }

       /*[CanBeNull]*/
        public ICspConfiguration GetCspConfigWithOverrides(/*[NotNull]*/IHttpContextWrapper context, bool reportOnly)
        {

            var overrides = _contextConfigurationHelper.GetCspConfigurationOverride(context, reportOnly, true);

            //No overrides
            if (overrides == null)
            {
                return null;
            }

            var newConfig = new CspConfiguration(false);
            var originalConfig = _contextConfigurationHelper.GetCspConfiguration(context, reportOnly);

            //We might be "attributes only", so no other config around.
            if (originalConfig != null)
            {
                _configMapper.MergeConfiguration(originalConfig, newConfig);
            }

            //Deal with header override
            _configMapper.MergeOverrides(overrides, newConfig);

            return newConfig;

        }

        internal void SetCspHeaderOverride(IHttpContextWrapper context, ICspHeaderConfiguration cspConfig, bool reportOnly)
        {
            var overrides = _contextConfigurationHelper.GetCspConfigurationOverride(context, reportOnly, false);

            overrides.EnabledOverride = true;
            overrides.Enabled = cspConfig.Enabled;
        }

        internal void SetCspDirectiveOverride(IHttpContextWrapper context, CspDirectives directive, CspDirectiveOverride config, bool reportOnly)
        {
            var overrides = _contextConfigurationHelper.GetCspConfigurationOverride(context, reportOnly, false);

            var directiveToOverride = _configMapper.GetCspDirectiveConfig(overrides, directive);

            if (directiveToOverride == null)
            {
                var baseConfig = _contextConfigurationHelper.GetCspConfiguration(context, reportOnly);
                directiveToOverride = _configMapper.GetCspDirectiveConfigCloned(baseConfig, directive);
            }

            var newConfig = _cspDirectiveOverrideHelper.GetOverridenCspDirectiveConfig(config, directiveToOverride);

            _configMapper.SetCspDirectiveConfig(overrides, directive, newConfig);
        }

        public void SetCspPluginTypesOverride(IHttpContextWrapper context, CspPluginTypesOverride config, bool reportOnly)
        {
            var overrides = _contextConfigurationHelper.GetCspConfigurationOverride(context, reportOnly, false);

            var directiveToOverride = overrides.PluginTypesDirective;

            if (directiveToOverride == null)
            {
                var baseConfig = _contextConfigurationHelper.GetCspConfiguration(context, reportOnly);
                directiveToOverride = _configMapper.GetCspPluginTypesConfigCloned(baseConfig);
            }

            var newConfig = _cspDirectiveOverrideHelper.GetOverridenCspPluginTypesConfig(config, directiveToOverride);

            overrides.PluginTypesDirective = newConfig;
        }

        internal void SetCspSandboxOverride(IHttpContextWrapper context, CspSandboxOverride config, bool reportOnly)
        {
            var overrides = _contextConfigurationHelper.GetCspConfigurationOverride(context, reportOnly, false);

            var directiveToOverride = overrides.SandboxDirective;

            if (directiveToOverride == null)
            {
                var baseConfig = _contextConfigurationHelper.GetCspConfiguration(context, reportOnly);
                directiveToOverride = _configMapper.GetCspSandboxConfigCloned(baseConfig);
            }

            var newConfig = _cspDirectiveOverrideHelper.GetOverridenCspSandboxConfig(config, directiveToOverride);

            overrides.SandboxDirective = newConfig;
        }

        public void SetCspMixedContentOverride(IHttpContextWrapper context, CspMixedContentOverride config, Boolean reportOnly)
        {
            var overrides = _contextConfigurationHelper.GetCspConfigurationOverride(context, reportOnly, false);

            var directiveToOverride = overrides.MixedContentDirective;

            if (directiveToOverride == null)
            {
                var baseConfig = _contextConfigurationHelper.GetCspConfiguration(context, reportOnly);
                directiveToOverride = _configMapper.GetCspMixedContentConfigCloned(baseConfig);
            }

            var newConfig = _cspDirectiveOverrideHelper.GetOverridenCspMixedContentConfig(config, directiveToOverride);

            overrides.MixedContentDirective = newConfig;
        }

        internal void SetCspReportUriOverride(IHttpContextWrapper context, ICspReportUriDirectiveConfiguration reportUriConfig, bool reportOnly)
        {
            var overrides = _contextConfigurationHelper.GetCspConfigurationOverride(context, reportOnly, false);

            overrides.ReportUriDirective = reportUriConfig;
        }

        public string GetCspScriptNonce(IHttpContextWrapper context)
        {
            var overrides = _contextConfigurationHelper.GetCspConfigurationOverride(context, false, false);

            if (overrides.ScriptSrcDirective != null && overrides.ScriptSrcDirective.Nonce != null)
            {
                return overrides.ScriptSrcDirective.Nonce;
            }

            var nonce = GenerateCspNonceValue();

            SetCspDirectiveNonce(context, nonce, CspDirectives.ScriptSrc, false);
            SetCspDirectiveNonce(context, nonce, CspDirectives.ScriptSrc, true);

            return nonce;
        }

        public string GetCspStyleNonce(IHttpContextWrapper context)
        {
            var overrides = _contextConfigurationHelper.GetCspConfigurationOverride(context, false, false);

            if (overrides.StyleSrcDirective != null && overrides.StyleSrcDirective.Nonce != null)
            {
                return overrides.StyleSrcDirective.Nonce;
            }

            var nonce = GenerateCspNonceValue();

            SetCspDirectiveNonce(context, nonce, CspDirectives.StyleSrc, false);
            SetCspDirectiveNonce(context, nonce, CspDirectives.StyleSrc, true);

            return nonce;
        }

        private void SetCspDirectiveNonce(IHttpContextWrapper context, string nonce, CspDirectives directive, bool reportOnly)
        {
            var overrides = _contextConfigurationHelper.GetCspConfigurationOverride(context, reportOnly, false);

            var directiveConfig = _configMapper.GetCspDirectiveConfig(overrides, directive);

            if (directiveConfig == null)
            {
                var baseConfig = _contextConfigurationHelper.GetCspConfiguration(context, reportOnly);
                directiveConfig = _configMapper.GetCspDirectiveConfigCloned(baseConfig, directive) ?? new CspDirectiveConfiguration();
                _configMapper.SetCspDirectiveConfig(overrides, directive, directiveConfig);
            }

            directiveConfig.Nonce = nonce;
        }

        private string GenerateCspNonceValue()
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                var nonceBytes = new byte[15];
                rng.GetBytes(nonceBytes);
                return Convert.ToBase64String(nonceBytes);
            }
        }
    }
}
