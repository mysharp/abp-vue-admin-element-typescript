﻿using LINGYUN.Abp.WeChat.Localization;
using Volo.Abp.Localization;
using Volo.Abp.Settings;

namespace LINGYUN.Abp.WeChat.MiniProgram.Settings
{
    public class WeChatMiniProgramSettingDefinitionProvider : SettingDefinitionProvider
    {
        public override void Define(ISettingDefinitionContext context)
        {
            context.Add(
                new SettingDefinition(
                    WeChatMiniProgramSettingNames.AppId, "",
                    L("DisplayName:WeChat.MiniProgram.AppId"),
                    L("Description:WeChat.MiniProgram.AppId"),
                    isVisibleToClients: true,
                    isEncrypted: true),
                new SettingDefinition(
                    WeChatMiniProgramSettingNames.AppSecret, "",
                    L("DisplayName:WeChat.MiniProgram.AppSecret"),
                    L("Description:WeChat.MiniProgram.AppSecret"),
                    isVisibleToClients: true,
                    isEncrypted: true),
                new SettingDefinition(
                    WeChatMiniProgramSettingNames.Token, "",
                    L("DisplayName:WeChat.MiniProgram.Token"),
                    L("Description:WeChat.MiniProgram.Token"),
                    isVisibleToClients: true,
                    isEncrypted: true),
                new SettingDefinition(
                    WeChatMiniProgramSettingNames.EncodingAESKey, "",
                    L("DisplayName:WeChat.MiniProgram.EncodingAESKey"),
                    L("Description:WeChat.MiniProgram.EncodingAESKey"),
                    isVisibleToClients: true,
                    isEncrypted: true)
            );
        }

        protected ILocalizableString L(string name)
        {
            return LocalizableString.Create<WeChatResource>(name);
        }
    }
}
