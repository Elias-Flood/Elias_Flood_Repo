Lite context.
Dessa två scripts är vad som tillsammans driver kubes rullnings systemet, som kan ses här: https://youtu.be/DdvkC4MqozU

RotationCheck håller majoriteten av logiken som förflyttar lådan, hur lådan ska roteras, kommunicerar med LevelManager, mm.
FollowMousePosition håller logiken för handtagen, när de har blivit greppade av spelaren, hur de följer spelarens händer i VR, mm. 

De kommunicerar mellan varandra för att veta om ett handtag har blivit greppat, vilket handtag lådan ska följa, när och hur lådan ska tippa.
