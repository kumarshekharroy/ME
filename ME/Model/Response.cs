﻿using System;
using System.Collections.Generic;
using System.Text;

namespace ME.Model
{
    public class Response<T>
    {
        public string status { get; set; }
        public string statuscode { get; set; }
        public string message { get; set; }
        public T data { get; set; }

    }
}
