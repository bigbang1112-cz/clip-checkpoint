﻿namespace GbxToolAPI.Client.Services;

public class SettingsService
{
    public int MaxFileCountToUpload { get; set; } = int.MaxValue;
    public int MaxFileSizeToUpload { get; set; } = int.MaxValue;
    public bool AcceptCustomDownloads { get; set; }
}
