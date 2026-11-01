from PIL import Image,ImageDraw,ImageFont,ImageFilter
FB='/usr/share/fonts/truetype/dejavu/DejaVuSans-Bold.ttf'
im=Image.new('RGB',(512,456),'white'); d=ImageDraw.Draw(im)
d.rectangle([6,6,505,449],outline=(200,20,20),width=12)
d.polygon([(256,30),(356,200),(156,200)],fill=(250,205,0)); d.line([(256,30),(356,200),(156,200),(256,30)],fill='black',width=8,joint='curve')
d.polygon([(262,75),(232,140),(258,140),(240,185),(285,122),(259,122),(276,75)],fill='black')
def ctext(y,t,s,c):
    f=ImageFont.truetype(FB,s); w=d.textlength(t,font=f); d.text(((512-w)/2,y),t,font=f,fill=c)
ctext(212,'DANGER',72,(200,20,20)); ctext(290,'DE MORT',72,(200,20,20))
ctext(378,'Bassin de rétention - montée',24,'black'); ctext(408,'brutale des eaux - accès interdit',24,'black')
im.filter(ImageFilter.GaussianBlur(0.6)).save('/workspace/ValVertGame/Assets/ValVert/Gen/sign_danger.png')
