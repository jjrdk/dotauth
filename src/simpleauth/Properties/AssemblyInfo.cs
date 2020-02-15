// Copyright © 2015 Habart Thierry, © 2018 Jacob Reimers
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("simpleauth.sms")]
[assembly: InternalsVisibleTo("simpleauth.tests")]
[assembly: InternalsVisibleTo("simpleauth.acceptancetests")]
[assembly: InternalsVisibleTo("simpleauth.stores.marten")]
[assembly: InternalsVisibleTo("simpleauth.stores.marten.acceptancetests")]
[assembly: InternalsVisibleTo("simpleauth.stores.redis.acceptancetests")]
[assembly: InternalsVisibleTo("simpleauth.server.tests")]
[assembly: InternalsVisibleTo("simpleauth.twilio.tests")]
