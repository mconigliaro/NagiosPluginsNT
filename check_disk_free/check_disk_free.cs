/* check_disk_free.cs - Disk space utilization (free) check plugin
 * 
 * NagiosPluginsNT - Nagios NRPE plugins for Windows NT
 * Copyright (c) 2009, Michael T. Conigliaro
 * 
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License version 2 as 
 * published by the Free Software Foundation.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License along
 * with this program; if not, write to the Free Software Foundation, Inc.,
 * 51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.
 */

using System;
using System.Collections.Generic;
using System.Text;
using NagiosPluginsNT;
using Mono.GetOptions;
using System.Management;

namespace check_disk_free
{    
    public class CheckDiskFreeWmiPluginOptions : WmiPluginOptions
    {
        [Option("Disk (default: all fixed)", 'd')]
        public string disk;
    }

    class check_disk_free
    {
        static void Main(string[] args)
        {
            CheckDiskFreeWmiPluginOptions Options = new CheckDiskFreeWmiPluginOptions();
            Options.label = "Disk Free";            
            Options.units = "GB";
            Options.multiplier = .000000001M;
            Options.ProcessArgs(args);

            WmiPlugin plugin = new WmiPlugin(Options);
            plugin.Connect();

            if (Options.disk != null)
            {
                plugin.Query(String.Format("Select DeviceID, FreeSpace From Win32_LogicalDisk where DeviceID='{0}' and FreeSpace is not null", Options.disk));
            }
            else
            {
                plugin.Query("Select DeviceID, FreeSpace From Win32_LogicalDisk where DriveType=3 and FreeSpace is not null");
            }
            if (plugin.ResultCollection.Count > 0)
            {
                foreach (ManagementObject mgtObject in plugin.ResultCollection)
                {
                    if (mgtObject["FreeSpace"] != null)
                    {
                        plugin.AppendValue(mgtObject["DeviceID"].ToString(), Convert.ToDecimal(mgtObject["FreeSpace"].ToString()));
                    }
                }
            }
            else
            {
                plugin.FatalError(Plugin<WmiPluginOptions>.StatusCode.Critical, "", "WMI query returned 0 results");
            }
            plugin.Finish();
        }
    }
}
