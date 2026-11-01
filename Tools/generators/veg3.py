import json,numpy as np
from PIL import Image,ImageDraw
from scipy import ndimage as nd
f=json.load(open('/workspace/valvert/fit.json'))
W,H=1496,709; X0,Z1=-532.0,354.646   # 1 m grid, north-up
def px(la,lo): x=(lo-f['clon'])*f['kx']; z=(la-f['clat'])*f['ky']; return (x-X0)*W/1496.1,(Z1-z)*H/709.293
d=json.load(open('big.json'))['elements']; N={e['id']:(e['lat'],e['lon']) for e in d if e['type']=='node'}
bm=Image.new('L',(W,H)); db=ImageDraw.Draw(bm); rm=Image.new('L',(W,H)); dr=ImageDraw.Draw(rm)
for e in d:
    if e['type']!='way': continue
    t=e.get('tags',{}); p=[px(*N[n]) for n in e['nodes'] if n in N]
    if len(p)<2: continue
    if 'building' in t or 'building:part' in t:
        if len(p)>2: db.polygon(p,fill=255)
    hw=t.get('highway')
    if hw:
        w={'primary':10,'secondary':9,'tertiary':8,'residential':6,'unclassified':6,'service':4}.get(hw,2.5)
        dr.line(p,fill=255,width=int(w))
B=nd.binary_dilation(np.asarray(bm)>0,iterations=2); Rd=np.asarray(rm)>0
mnh=np.array(Image.open('mnh.tif')).astype(np.float32); mnh[B]=0
chm=nd.gaussian_filter(mnh,1.0)
mx=nd.maximum_filter(chm,size=7)
tops=(chm==mx)&(chm>4.5)
ys,xs=np.nonzero(tops)
canopy=nd.binary_closing(chm>2.5,iterations=2)&~B
lab,n=nd.label(canopy); sz=nd.sum(canopy,lab,range(1,n+1))
trees=[]
for y,x in zip(ys,xs):
    h=float(chm[y,x]); X=X0+x+0.5; Z=Z1-y-0.5
    # trunk not on road: if on road, look for nearest off-road canopy pixel within 4 m
    if Rd[y,x]:
        win=Rd[max(0,y-4):y+5,max(0,x-4):x+5]; o=np.argwhere(~win)
        if len(o)==0: continue
        oy,ox=o[np.argmin(((o-[4,4])**2).sum(1))]; X+=ox-4; Z-=oy-4
    big=sz[lab[y,x]-1]>400 if lab[y,x] else False
    trees.append([round(X,2),round(Z,2),round(h,1),int(big)])
shr=(chm>0.8)&(chm<2.5)&~B&~Rd; sy,sx=np.nonzero(shr); sel=np.random.default_rng(1).random(len(sy))<0.12
shrubs=[[round(X0+x+.5,2),round(Z1-y-.5,2)] for y,x in zip(sy[sel],sx[sel])]
json.dump({'trees':trees,'shrubs':shrubs},open('veg3.json','w'))
Image.fromarray((canopy*255).astype(np.uint8)).save('canopy3.png')
np.save('chm.npy',chm)
t=np.array(trees); print('trees',len(t),'in orig',(t[:,0]<532).sum(),'forest(big blob)',t[:,3].sum(),'h pct',np.percentile(t[:,2],[10,50,90]),'shrubs',len(shrubs),'canopy%',canopy.mean()*100)
# infill understory in big canopy blobs
from scipy.spatial import cKDTree
rng=np.random.default_rng(7); kd=cKDTree(t[:,:2]); inf=[]
bigm=np.isin(lab,np.nonzero(sz>400)[0]+1)
for _ in range(60000):
    x=rng.uniform(0,W); y=rng.uniform(0,H); xi,yi=int(x),int(y)
    if not bigm[yi,xi] or Rd[yi,xi]: continue
    X,Z=X0+x,Z1-y
    if kd.query([X,Z])[0]<3.2: continue
    if inf and min(((np.array(inf[-400:])[:,:2]-[X,Z])**2).sum(1))<3.0**2: continue
    inf.append([round(X,2),round(Z,2),round(float(min(chm[yi,xi],rng.uniform(5,11))),1),2])
print('infill',len(inf))
json.dump({'trees':trees+inf,'shrubs':shrubs},open('veg3.json','w'))
