using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;

namespace TorDNSd
{
    partial class TorDNSdService : ServiceBase
    {
        public TorDNSdService(string[] args)
        {
            InitializeComponent();
        }

        public TorDNSdService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {

        }

        protected override void OnStop()
        {

        }
    }
}
