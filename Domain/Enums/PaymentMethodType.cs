﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;

namespace Domain.Enums
{
    public enum PaymentMethodType
    {
        Bank,
        VodafoneCash,
        EtisalatCash,
        OrangeCash
    }
}
