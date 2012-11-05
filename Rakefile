require 'albacore'

task :default => :output do
end

desc "Assembly info generator"
assemblyinfo :assemblyinfo do |asm|
  asm.version = "1.0.0.0"
  asm.file_version = "1.0.0.0"
  asm.company_name = "TRock"
  asm.product_name = "TRock.Music"
  asm.description = "Music streaming"
  asm.copyright = "Copyright 2012"  
  asm.custom_attributes :AssemblyInformationalVersionAttribute => "1.0.0.0 RC1"
  asm.output_file = "src/CommonAssemblyInfo.cs"
end

desc "Build solution"
msbuild :build => :assemblyinfo do |msb|
  msb.properties = { :configuration => :Release }
  msb.targets = [ :Clean, :Build ]
  msb.solution = "TRock.Music.sln"
end

exec :merge => :build do |cmd|
  cmd.command = "merge.bat"     
end

output :output => :merge do |out|
  out.from '.'
  out.to 'out'
  out.file 'src\TRock.Music\bin\Release\TRock.Music.dll', :as=>'TRock.Music.dll'  
  
  out.file 'src\TRock.Music.Grooveshark\bin\TRock.Music.Grooveshark.dll', :as=>'Grooveshark\TRock.Music.Grooveshark.dll'
  out.file 'packages\Newtonsoft.Json.4.5.10\lib\net40\Newtonsoft.Json.dll', :as=>'Grooveshark\Newtonsoft.Json.dll' 
  out.file 'packages\NAudio.1.6\lib\net20\NAudio.dll', :as=>'Grooveshark\NAudio.dll' 
  
  out.file 'src\TRock.Music.Spotify\bin\Release\TRock.Music.Spotify.dll', :as=>'Spotify\TRock.Music.Spotify.dll'
  out.file 'src\TRock.Music.Torshify\bin\Release\TRock.Music.Torshify.dll', :as=>'Spotify\TRock.Music.Torshify.dll'    
  out.file 'packages\Newtonsoft.Json.4.5.10\lib\net40\Newtonsoft.Json.dll', :as=>'Spotify\Newtonsoft.Json.dll'
  out.file 'packages\Microsoft.Net.Http.2.0.20710.0\lib\net40\System.Net.Http.dll', :as=>'Spotify\System.Net.Http.dll'
  out.file 'packages\SignalR.Client.0.5.3\lib\net40\SignalR.Client.dll', :as=>'Spotify\SignalR.Client.dll'
  
  out.file 'src\TRock.Music.Reactive\bin\Release\TRock.Music.Reactive.dll', :as=>'Reactive\TRock.Music.Reactive.dll' 
  out.file 'packages\Rx-Core.2.0.20622-rc\lib\Net40\System.Reactive.Core.dll', :as=>'Reactive\System.Reactive.Core.dll'
  out.file 'packages\Rx-Interfaces.2.0.20622-rc\lib\Net40\System.Reactive.Interfaces.dll', :as=>'Reactive\System.Reactive.Interfaces.dll'
  out.file 'packages\Rx-Linq.2.0.20622-rc\lib\Net40\System.Reactive.Linq.dll', :as=>'Reactive\System.Reactive.Linq.dll'
  out.file 'packages\Rx-PlatformServices.2.0.20622-rc\lib\Net40\System.Reactive.PlatformServices.dll', :as=>'Reactive\System.Reactive.PlatformServices.dll'
    
  out.file 'src\TRock.Music.Torshify.Server\bin\Release\TRock.Music.Torshify.Server.exe', :as=>'SpotifyServer\TRock.Music.Torshify.Server.exe' 
  out.file 'src\TRock.Music.Torshify.Server\bin\Release\TRock.Music.dll', :as=>'SpotifyServer\TRock.Music.dll' 
  out.file 'src\TRock.Music.Torshify.Server\bin\Release\libspotify.dll', :as=>'SpotifyServer\libspotify.dll' 
end