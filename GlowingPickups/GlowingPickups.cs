﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using GTA;
using GTA.Native;
using GTA.Math;
using System.Runtime.InteropServices;

namespace GlowingPickups
{
    public class GlowingPickups : Script
    {
        Setting settings;

        public GlowingPickups()
        {
            var xmlPath = Path.ChangeExtension((new Uri(Assembly.GetExecutingAssembly().CodeBase)).LocalPath, "xml");
            var settingLoader = new SettingLoader<Setting>();
            settings = settingLoader.Load(xmlPath) ?? settingLoader.Init(xmlPath);
            PickupObjectPoolTask.Init();

            Tick += OnTick;
            Interval = 0;
        }

        private void OnTick(object o, EventArgs e)
        {
            if (settings == null)
            {
                return;
            }

            var offset = (int)Game.Version >= (int)GameVersion.VER_1_0_944_2_STEAM ? 0x480 : 0x470;

            foreach (var pickupAddr in PickupObjectPoolTask.GetPickupObjectAddresses())
            {
                unsafe
                {
                    var isVisible = (Marshal.ReadByte(pickupAddr, 0x2C) & 0x01) == 1;

                    if (!isVisible)
                    {
                        continue;
                    }

                    var pos = *(Vector3*)(pickupAddr + 0x90);
                    var dataAddress = Marshal.ReadIntPtr(pickupAddr, offset);

                    if (dataAddress != IntPtr.Zero)
                    {
                        var red = (int)(BitConverter.ToSingle(
                            BitConverter.GetBytes(Marshal.ReadInt32(dataAddress, 0x5C)), 0) * 255);
                        var green = (int)(BitConverter.ToSingle(
                            BitConverter.GetBytes(Marshal.ReadInt32(dataAddress, 0x60)), 0) * 255);
                        var blue = (int)(BitConverter.ToSingle(
                            BitConverter.GetBytes(Marshal.ReadInt32(dataAddress, 0x64)), 0) * 255);
                        var range = BitConverter.ToSingle(
                            BitConverter.GetBytes(Marshal.ReadInt32(dataAddress, 0x10)), 0) * settings.RangeMultiplier;
                        var intensity = BitConverter.ToSingle(
                            BitConverter.GetBytes(Marshal.ReadInt32(dataAddress, 0x68)), 0) * settings.LightIntensityMultiplier;
                        var darkIntensity = BitConverter.ToSingle(
                            BitConverter.GetBytes(Marshal.ReadInt32(dataAddress, 0x6C)), 0) * settings.ShadowMultiplier;
                        Function.Call(Hash._DRAW_LIGHT_WITH_RANGE_WITH_SHADOW, pos.X, pos.Y, pos.Z, red,
                        green, blue, range, intensity, darkIntensity);
                    }
                    else
                    {
                        Function.Call(Hash._DRAW_LIGHT_WITH_RANGE_WITH_SHADOW, pos.X, pos.Y, pos.Z, 255, 57, 0, 5.0f, 30.0f, 10.0f);
                    }
                }
            }

            // AddEntityToPoolFunc in GetPickupObjectHandles() isn't working

            /*  var pickupProps = PickupObjectPoolTask.GetPickupObjectHandles();
                foreach (var pickup in pickupProps)
            {
                //0x470 - in biker update

                var offset = (int)Game.Version >= (int)GameVersion.VER_1_0_944_2_STEAM ? 0x480 : 0x470;

                unsafe
                {
                    var dataAddress = Marshal.ReadIntPtr(new IntPtr(pickup.MemoryAddress), offset);
                    if (dataAddress != IntPtr.Zero)
                    {

                        //Color glowingColor;
                        var pos = pickup.Position;
                        var red = (int)(BitConverter.ToSingle(
                            BitConverter.GetBytes(Marshal.ReadInt32(dataAddress, 0x5C)), 0) * 255);
                        var green = (int)(BitConverter.ToSingle(
                            BitConverter.GetBytes(Marshal.ReadInt32(dataAddress, 0x60)), 0) * 255);
                        var blue = (int)(BitConverter.ToSingle(
                            BitConverter.GetBytes(Marshal.ReadInt32(dataAddress, 0x64)), 0) * 255);
                        var range = BitConverter.ToSingle(
                            BitConverter.GetBytes(Marshal.ReadInt32(dataAddress, 0x10)), 0);
                        var intensity = BitConverter.ToSingle(
                            BitConverter.GetBytes(Marshal.ReadInt32(dataAddress, 0x68)), 0) * 3f;
                        var darkIntensity = BitConverter.ToSingle(
                            BitConverter.GetBytes(Marshal.ReadInt32(dataAddress, 0x6C)), 0) * 3f;
                        Function.Call(Hash._DRAW_LIGHT_WITH_RANGE_WITH_SHADOW, pos.X, pos.Y, pos.Z, red,
                        green, blue, range, intensity, darkIntensity);
                    }
                    else
                    {
                        var pos = pickup.Position;
                        Function.Call(Hash._DRAW_LIGHT_WITH_RANGE_WITH_SHADOW, pos.X, pos.Y, pos.Z, 255, 57, 0, 5.0f, 30.0f, 10.0f);
                    }
                }
            }*/
        }
    }
}
