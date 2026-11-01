# self-made facade atlas (CC0 grey_plaster base from Poly Haven, tinted) : 6 tones x 8 element rows, cell=256px = 3.0m x 2.8m
from PIL import Image, ImageDraw, ImageFilter
import numpy as np, random
G='/workspace/ValVertGame/Assets/ValVert/Gen/'
C=256; TONES=[(234,224,198),(222,202,165),(208,207,201),(230,204,188),(236,226,190),(240,238,232)]
SHUT=[(110,72,44),(70,96,72),(150,150,145),(120,60,50),(90,110,130),(230,230,225)]
pl=np.asarray(Image.open(G+'grey_plaster_diff_1k.jpg').convert('L').resize((C,C)),np.float32)/255.
pl=np.clip(0.9+0.035*(pl-pl.mean())/max(pl.std(),1e-3),0.8,1.0)
A=Image.new('RGB',(C*6,C*16)); RR=random.Random(7)
def win(d,x0,y0,x1,y1,shut=None,roller=0.0):
    d.rectangle([x0-4,y0-4,x1+4,y1+6],fill=(0,0,0,0))
    d.rectangle([x0-6,y1+2,x1+6,y1+10],fill=(205,203,196))            # sill
    d.rectangle([x0,y0,x1,y1],fill=(245,245,242))                      # PVC frame
    g=[x0+7,y0+7,x1-7,y1-7]
    for yy in range(g[1],g[3]+1):
        t=(yy-g[1])/max(1,g[3]-g[1]); c=(int(70+60*(1-t)),int(82+62*(1-t)),int(95+60*(1-t))); d.line([g[0],yy,g[2],yy],fill=c)
    v=RR.random()
    if v<0.22:   # net curtains (voilage), very 2010
        cc=RR.choice([(225,222,212),(210,205,190),(232,230,226)]); d.rectangle([g[0],g[1]+int((g[3]-g[1])*0.5),g[2],g[3]],fill=cc)
        for xx in range(g[0],g[2],4): d.line([xx,g[1]+int((g[3]-g[1])*0.25),xx,g[3]],fill=tuple(c-18 for c in cc))
    elif v<0.5:  # side drapes
        cc=RR.choice([(150,60,50),(120,110,90),(70,80,110),(190,170,120)]); w_=(g[2]-g[0])//5
        d.rectangle([g[0],g[1],g[0]+w_,g[3]],fill=cc); d.rectangle([g[2]-w_,g[1],g[2],g[3]],fill=cc)
    elif v<0.75:   # sky reflection patch
        d.polygon([(g[0],g[1]),(g[0]+(g[2]-g[0])//2,g[1]),(g[0],g[1]+(g[3]-g[1])//2)],fill=(150,165,180))
    d.rectangle([x0,y0,x1,y0+5],fill=(60,58,55))                      # recess shadow
    d.rectangle([x0,y0,x0+4,y1],fill=(90,88,84))
    mx=(x0+x1)//2; d.rectangle([mx-3,y0,mx+3,y1],fill=(245,245,242))
    if roller>0: d.rectangle([x0+7,y0+7,x1-7,y0+7+int((y1-y0-14)*roller)],fill=(200,198,190))
    if roller>0:
        for yy in range(y0+9,y0+7+int((y1-y0-14)*roller),5): d.line([x0+7,yy,x1-7,yy],fill=(170,168,160))
    if shut:
        w=(x1-x0)//2+2
        for sx in (x0-w-6,x1+6):
            d.rectangle([sx,y0,sx+w,y1],fill=shut)
            for yy in range(y0+6,y1,9): d.line([sx+3,yy,sx+w-3,yy],fill=tuple(int(c*0.75) for c in shut))
def door(d,x0,y0,x1,y1,col):
    d.rectangle([x0-6,y0-6,x1+6,y1],fill=(215,212,205)); d.rectangle([x0,y0,x1,y1],fill=col)
    d.rectangle([x0+10,y0+12,x1-10,y0+70],fill=(80,92,100)); d.ellipse([x1-20,(y0+y1)//2,x1-12,(y0+y1)//2+8],fill=(200,190,120))
def plinth(d): d.rectangle([0,C-34,C,C],fill=(150,148,142)); d.line([0,C-34,C,C-34],fill=(120,118,112),width=3)
for ti,tone in enumerate(TONES):
    base=(pl[...,None]*np.array(tone)[None,None,:]).clip(0,255).astype(np.uint8)
    for row in range(16):
        im=Image.fromarray(base.copy()); d=ImageDraw.Draw(im); sh=SHUT[ti]
        if row==0: win(d,92,52,164,190,shut=sh)
        if row==1: door(d,88,48,168,C-30,(110,80,55) if ti%2 else (240,240,236)); plinth(d)
        if row==2: pass
        if row==3: win(d,70,60,186,186,roller=0.25+0.15*(ti%3))
        if row==4: win(d,58,30,198,C-8,roller=0.15)
        if row==5:
            for k in range(4): d.rectangle([116,20+k*58,140,62+k*58],fill=(95,110,120)); d.rectangle([112,16+k*58,144,20+k*58],fill=(200,198,192))
        if row==6: d.rectangle([40,40,216,C],fill=(205,203,196)); d.rectangle([52,52,204,C],fill=(70,85,95)); d.line([128,52,128,C],fill=(180,180,175),width=5); d.rectangle([40,24,216,40],fill=(120,120,118)); plinth(d)
        if row==7: win(d,92,60,164,180,shut=sh); plinth(d)
        if row==8: win(d,104,80,152,140)                                    # small bathroom window
        if row==9:
            win(d,150,90,200,140)
            d.rectangle([60,100,96,128],fill=(185,183,176))
            for yy in range(103,127,4): d.line([62,yy,94,yy],fill=(120,118,112))       # vent grille
        if row==10:
            d.rectangle([118,0,130,C],fill=(150,150,146)); d.line([120,0,120,C],fill=(185,185,180),width=2)  # downpipe
            for yy in (40,130,220): d.rectangle([114,yy,134,yy+5],fill=(120,120,116))
            d.rectangle([60,120,82,140],fill=(170,168,160))
        if row==11:
            d.rectangle([30,150,62,176],fill=(205,203,196)); d.ellipse([38,156,54,170],fill=(110,110,108))   # wall-light / meter box
            d.rectangle([180,120,220,175],fill=(200,198,190)); d.rectangle([184,124,216,171],outline=(150,148,140))
        if row==12: win(d,70,60,186,186,roller=0.92)
        if row==13: win(d,70,60,186,186,roller=0.0)
        if row==14: win(d,70,60,186,186,roller=0.55)
        if row==15: win(d,92,52,164,190,roller=0.6)
        if row==2: d.line([0,C-2,C,C-2],fill=tuple(int(c*0.92) for c in tone),width=3)
        # weathering: grime near the base + streaks under sills/openings
        a=np.asarray(im,np.float32); h=np.linspace(0,1,C)[:,None,None]
        rnd=np.random.default_rng(ti*31+row); nz=rnd.random((C//8,C//8)); nz=np.asarray(Image.fromarray((nz*255).astype(np.uint8)).resize((C,C),Image.BILINEAR),np.float32)[...,None]/255
        if row in (1,6,7,2,8,9,10,11,3,12,13,14,15): a*=1-0.10*np.clip((h-0.82)/0.18,0,1)*(0.6+0.4*nz)
        a*=1-0.04*(nz-0.5)
        if row in (0,3,4,7,8,9):
            ys={0:196,3:194,4:C-2,7:186,8:146,9:146}[row]; xs={0:(92,164),3:(70,186),4:(58,198),7:(92,164),8:(104,152),9:(150,200)}[row]
            for xx in range(xs[0]+4,xs[1]-2,7):
                ln=int(rnd.uniform(15,60)); w=rnd.uniform(0.03,0.08)
                if ys+ln<C: a[ys:ys+ln,xx:xx+3]*=(1-w*np.linspace(1,0,ln))[:,None,None]
        im=Image.fromarray(a.clip(0,255).astype(np.uint8))
        A.paste(im,(ti*C,row*C))
A.save(G+'facade_atlas.png')
print('atlas ok')
