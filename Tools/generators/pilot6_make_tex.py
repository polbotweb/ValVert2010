# pilot6: Yughues forest ground 02 (CC0) -> 2K; ambientCG LeafSet022 (CC0) -> laurel cluster card 1024 RGBA + normal
import numpy as np; from PIL import Image; from scipy import ndimage
O='/workspace/ValVertGame/Assets/ValVert/Veg/pilot6/'; import os; os.makedirs(O,exist_ok=True)
d=Image.open('fg/_diffuse.tga').convert('RGB').resize((2048,2048),Image.LANCZOS)
ao=np.array(Image.open('fg/_occlusion.tga').convert('L').resize((2048,2048)),float)/255
da=np.array(d,float)*(0.75+0.25*ao)[...,None]*1.35
Image.fromarray(np.clip(da,0,255).astype(np.uint8)).save(O+'fg02_diff.jpg',quality=90)
Image.open('fg/_normals.tga').convert('RGB').resize((2048,2048),Image.LANCZOS).save(O+'fg02_nor.jpg',quality=92)
# leaves
P='leaf22/LeafSet022_2K-PNG_'
col=np.array(Image.open(P+'Color.png').convert('RGB'),float); op=np.array(Image.open(P+'Opacity.png').convert('L'),float)/255
nor=np.array(Image.open(P+'NormalGL.png').convert('RGB'),float)
lab,n=ndimage.label(op>0.5); objs=ndimage.find_objects(lab); print('leaves',n)
leaves=[]
for i,s in enumerate(objs):
    if (s[0].stop-s[0].start)*(s[1].stop-s[1].start)<4000: continue
    m=(lab[s]==i+1).astype(float)*op[s]; leaves.append((col[s],nor[s],m))
print('usable',len(leaves))
S=1024; C=np.zeros((S,S,3)); A=np.zeros((S,S)); N=np.zeros((S,S,3)); N[...]=(128,128,255)
r=np.random.default_rng(7)
for k in range(95):
    c,nn,m=leaves[r.integers(len(leaves))]
    sc=r.uniform(0.14,0.20)*S/max(c.shape[:2]); ang=r.uniform(0,360)
    def tf(a,mode='RGB'):
        im=Image.fromarray(np.clip(a,0,255).astype(np.uint8),mode); im=im.resize((max(2,int(im.width*sc)),max(2,int(im.height*sc))),Image.LANCZOS); return np.array(im.rotate(ang,expand=True,resample=Image.BICUBIC),float)
    cc=tf(c); mm=tf(m*255,'L')/255; nb=tf(nn)
    h,w=mm.shape; cx=int(r.uniform(0.12,0.88)*S-w/2); cy=int(r.uniform(0.12,0.88)*S-h/2)
    x0,y0=max(cx,0),max(cy,0); x1,y1=min(cx+w,S),min(cy+h,S)
    if x1<=x0 or y1<=y0: continue
    sl=(slice(y0,y1),slice(x0,x1)); ss=(slice(y0-cy,y1-cy),slice(x0-cx,x1-cx))
    a=mm[ss][...,None]
    shade=r.uniform(0.75,1.05)
    C[sl]=C[sl]*(1-a)+cc[ss]*shade*a; N[sl]=N[sl]*(1-a)+nb[ss]*a; A[sl]=np.maximum(A[sl],mm[ss])
# laurel tone: Prunus laurocerasus dark glossy green -> darken & shift toward deep green, keep scan detail
g=C.mean(-1,keepdims=True); C=(C*0.75+g*0.25)*np.array([0.42,0.56,0.40])
out=np.dstack([np.clip(C,0,255),A*255]).astype(np.uint8)
# bleed colour into transparent area to avoid dark fringes
rgb=out[...,:3].astype(float); msk=A>0.5
idx=ndimage.distance_transform_edt(~msk,return_distances=False,return_indices=True); rgb=rgb[idx[0],idx[1]]; out[...,:3]=rgb.astype(np.uint8)
Image.fromarray(out,'RGBA').save(O+'laurel_acg_leaves.png')
Image.fromarray(np.clip(N,0,255).astype(np.uint8)).save(O+'laurel_acg_nor.png')
