require 'albacore'

task :default => :build do
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
  out.file 'src\TRock.Music.Grooveshark\bin\TRock.Music.Grooveshark.dll', :as=>'TRock.Music.Grooveshark.dll'
  out.file 'src\TRock.Music.Spotify\bin\Release\TRock.Music.Spotify.dll', :as=>'TRock.Music.Spotify.dll'
  out.file 'src\TRock.Music.Torshify\bin\Release\TRock.Music.Torshify.dll', :as=>'TRock.Music.Torshify.dll'  
  out.file 'src\TRock.Music.Reactive\bin\Release\TRock.Music.Reactive.dll', :as=>'TRock.Music.Reactive.dll' 
end

