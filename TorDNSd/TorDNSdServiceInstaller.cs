using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;


namespace TorDNSd
{
    [RunInstaller(true)]
    public partial class TorDNSdServiceInstaller : System.Configuration.Install.Installer
    {
        public TorDNSdServiceInstaller()
        {
            InitializeComponent();
        }
    }
}
