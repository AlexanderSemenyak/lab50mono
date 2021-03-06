//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Activities.Presentation.Converters
{
    using System.Activities.Presentation.Converters;
    using System.Collections.Generic;
    using System.ServiceModel.Activities;

    class SendMessageContentSearchableStringConverter : SearchableStringConverter
    {
        public override IList<string> Convert(object value)
        {
            List<string> results = new List<string>();
            SendMessageContent content = value as SendMessageContent;
            if (null != content)
            {
                results.AddRange(new ArgumentSearchableStringConverter().Convert(content.Message));
                results.AddRange(new TypeSearchableStringConverter().Convert(content.DeclaredMessageType));
            }
            return results;
        }
    }
}
