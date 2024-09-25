adb logcat Unity:W '*:S'

adb exec-out "while true; do screenrecord --bit-rate=20m --output-format=h264 -; done" | vlc --demux h264 --h264-fps=60 --clock-jitter=0 -
