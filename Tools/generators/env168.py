# ---- #168 rue du Val Vert surroundings from the user's 2010-confirmed photos (ref_9valvert) + OSM footprints ----
def seg(m,a,b,r):   # square log prism between 3D points a,b (half-size r)
    a=np.array(a,float); b=np.array(b,float); d=b-a; L=np.linalg.norm(d)
    if L<1e-3: return
    d/=L; s1=np.cross(d,[0,1,0]); s1=s1/np.linalg.norm(s1) if np.linalg.norm(s1)>1e-3 else np.array([1.,0,0]); s2=np.cross(s1,d)
    cs=[s1*r+s2*r,-s1*r+s2*r,-s1*r-s2*r,s1*r-s2*r]
    for k in range(4):
        c0,c1=cs[k],cs[(k+1)%4]; n=(c0+c1)/2
        b_quad(m,tuple(a+c0),tuple(a+c1),tuple(b+c1),tuple(b+c0),[(0,0),(1,0),(1,L),(0,L)],n)
def post(q,h,r=0.07,m='pole_wood'): g_=Hc(*q); b_box(m,(q[0],g_-0.2,q[1]),(2*r,h+0.2,2*r),0)
def fence(pts,h=0.85,step=1.8,rails=(0.45,0.8)):   # rustic round-timber rail fence following the ground
    pts=[np.array(p_,float) for p_ in pts]
    for a,b in zip(pts[:-1],pts[1:]):
        L=np.linalg.norm(b-a); n=max(1,int(math.ceil(L/step)))
        for k in range(n+1):
            q=a+(b-a)*k/n; post(q,h)
            if k<n:
                q2=a+(b-a)*(k+1)/n
                for rh in rails: seg('pole_wood',(q[0],Hc(*q)+rh*h/0.85,q[1]),(q2[0],Hc(*q2)+rh*h/0.85,q2[1]),0.05)
_Ms=dict(M); M.clear(); nenv=dict(steps=0,posts=0)
# A) #168 (OSM 62591705): downpipes, glazed entrance doors with small canopy at nos 9/11/13
p168,g168,yb168,_,_=BI[62591705]; yt168=YT[62591705]
for (x,z) in [(373.5,133.0),(373.5,150.0),(374.7,163.0),(366.6,121.0),(388.2,120.0),(386.7,150.0)]:
    out=-1 if x<380 else 1; b_box('metal',(x+out*0.12,g168,z),(0.11,yt168-g168,0.11),0)
for (x,z) in [(373.5,128.5),(373.5,139.0),(374.6,156.2)]:
    yg=Hc(x-1,z); a=(x-0.03,z-0.9); b=(x-0.03,z+0.9)
    b_quad('glass_door',(a[0],yg,a[1]),(b[0],yg,b[1]),(b[0],yg+2.35,b[1]),(a[0],yg+2.35,a[1]),[(0,0),(1,0),(1,1),(0,1)],np.array([-1,0,0]))
    b_box('metal',(x-0.05,yg,z),(0.08,2.45,1.9),0); b_box('glass_door',(x-0.09,yg,z),(0.02,2.35,1.8),0)
    b_box('coping',(x-0.6,yg+2.6,z),(1.2,0.12,2.4),0)
# B) bin-store row (OSM 62592236, 3.5 x 12.3 m): grey render, grey metal doors, leaf-covered corrugated roof, gutter + downpipe
bs=np.array([[365.5,146.7],[362.0,146.9],[361.4,134.6],[364.9,134.4]]); gb=min(Hc(*q) for q in bs)-0.2; ytb=max(Hc(*q) for q in bs)+2.3
ctr=bs.mean(0); ax=(bs[0]-bs[3])/np.linalg.norm(bs[0]-bs[3]); yaw=math.atan2(ax[1],ax[0]); nx=np.array([ax[1],-ax[0]])   # nx: east (towards #168)
b_box('render_grey',(ctr[0],gb,ctr[1]),(12.3,ytb-gb,3.5),yaw)
rf=[ctr+ax*s1*6.55+nx*s2*2.05 for s1,s2 in((-1,-1),(1,-1),(1,1),(-1,1))]
yr=lambda q:ytb+0.25-0.25*np.dot(q-ctr,nx)/2.05   # mono-slope falling towards the path side
V4=[(q[0],yr(q),q[1]) for q in rf]
b_quad('corr_leaves',V4[0],V4[1],V4[2],V4[3],[(0,0),(4,0),(4,1.4),(0,1.4)],np.array([0,1.,0]))
b_quad('metal',(V4[0][0],V4[0][1]-0.08,V4[0][2]),(V4[1][0],V4[1][1]-0.08,V4[1][2]),(V4[2][0],V4[2][1]-0.08,V4[2][2]),(V4[3][0],V4[3][1]-0.08,V4[3][2]),[(0,0),(1,0),(1,1),(0,1)],np.array([0,-1.,0]))
gq=ctr+nx*2.12; b_box('metal',(gq[0],ytb-0.12,gq[1]),(13.1,0.14,0.16),yaw)          # gutter (path side)
dq=ctr+nx*2.1+ax*6.2; b_box('metal',(dq[0],gb,dq[1]),(0.1,ytb-gb,0.1),0)
for k in range(4):                                                                 # grey steel doors on the path side
    q=ctr+ax*(-4.6+k*3.07)+nx*1.77; b_box('metal',(q[0],gb+0.2,q[1]),(1.3,2.0,0.05),yaw); b_box('metal',(q[0]+ax[0]*0.45+nx[0]*0.04,gb+1.1,q[1]+ax[1]*0.45+nx[1]*0.04),(0.05,0.2,0.04),yaw)
HIDE.extend([float(v) for q in bs for v in q]); HIDEL.append(4)
# C) leaf/dirt ground between #168 and the woods, timber-edged planted beds, log staircase up to the OSM woods path 260433017
lg=[(354.0,117.5),(373.4,117.5),(373.4,166.0),(354.0,166.0)]
drape(lg,'leafground',0.06,lambda x,z:(x/3.0,z/3.0),1.0); HIDE.extend([v for q in lg for v in q]); HIDEL.append(4)
for z0,z1 in((119.0,126.0),(131.0,137.0),(142.0,153.5)):
    bed=[(369.6,z0),(373.3,z0),(373.3,z1),(369.6,z1)]; drape(bed,'bed_soil',0.1,lambda x,z:(x/2.0,z/2.0),0.5)
    fence([(373.3,z0),(369.6,z0),(369.6,z1),(373.3,z1)],h=0.55,step=2.2,rails=(0.4,))
    for k in range(int((z1-z0)/1.6)):
        q=(371.4+(k%2-0.5)*1.2,z0+0.8+k*1.6); b_box('hedge',(q[0],Hc(*q)-0.05,q[1]),(1.1,0.75,1.1),k*0.7)
A=np.array([361.0,148.3]); _pw=[np.array(W2(n_)) for n_ in WY[260433017]['nodes'] if n_ in ND]; _pp=[]
for a_,b_ in zip(_pw[:-1],_pw[1:]):
    for t_ in np.linspace(0,1,12): _pp.append(a_+(b_-a_)*t_)
_c=[((Hc(*q)-Hc(*A))/np.linalg.norm(q-A),q) for q in _pp if 6<np.linalg.norm(q-A)<20]
B=max(_c,key=lambda z:z[0])[1] if _c else np.array([351.3,144.5]); print('stair top candidates best slope',round(max(z[0] for z in _c),2) if _c else None)
hA=Hc(*A); hB=Hc(*B); L=np.linalg.norm(B-A); d=(B-A)/L; sd=np.array([-d[1],d[0]])
print('log stair', round(hA,2), round(hB,2), round(L,1))
ns=max(8,int(math.ceil(abs(hB-hA)/0.16))); 
for k in range(ns):
    q=A+d*L*(k+0.5)/ns; y=Hc(*q)
    y=hA+(hB-hA)*(k+0.5)/ns; y=max(y,Hc(*q)-0.05)
    seg('pole_wood',(q[0]-sd[0]*0.65,y+0.08,q[1]-sd[1]*0.65),(q[0]+sd[0]*0.65,y+0.08,q[1]+sd[1]*0.65),0.08); nenv['steps']+=1
    sq=[tuple(q-sd*0.62-d*L/ns*0.5),tuple(q+sd*0.62-d*L/ns*0.5),tuple(q+sd*0.62+d*L/ns*0.5),tuple(q-sd*0.62+d*L/ns*0.5)]
    b_quad('leafground',(sq[0][0],y+0.01,sq[0][1]),(sq[1][0],y+0.01,sq[1][1]),(sq[2][0],y+0.01,sq[2][1]),(sq[3][0],y+0.01,sq[3][1]),[(0,0),(.5,0),(.5,.5),(0,.5)],np.array([0,1.,0]))
st=[A+d*t for t in np.linspace(0,L,8)]
drape([tuple(A-sd*0.75),tuple(A+sd*0.75),tuple(B+sd*0.75),tuple(B-sd*0.75)],'leafground',0.09,lambda x,z:(x/2.0,z/2.0),0.5)
fence([A-sd*0.85,B-sd*0.85]); fence([A+sd*0.85,B+sd*0.85]); fence([(366.2,134.0),(366.2,118.5)])
# D) Maison rue de l'Orme (OSM 62594137): green glazed stairwell, maroon garage doors, rock-edged car park, lamp post
pm,gm,ybm,_,_=BI[62594137]; ytm=YT[62594137]; n=len(pm); best=None
for i in range(n):
    a=pm[i]; b=pm[(i+1)%n]; Lm=np.linalg.norm(b-a)
    if Lm<3: continue
    dd=(b-a)/Lm; out=np.array([dd[1],-dd[0]])
    if out[1]>0.7 and (best is None or Lm>best[0]): best=(Lm,a,b,dd,out)
Lm,a,b,dd,out=best; o3=np.array([out[0],0,out[1]])
c=(a+b)/2+out*0.06; q0=c-dd*1.3; q1=c+dd*1.3
b_quad('glass_green',(q0[0],gm+2.9,q0[1]),(q1[0],gm+2.9,q1[1]),(q1[0],ytm-0.3,q1[1]),(q0[0],ytm-0.3,q0[1]),[(0,0),(1,0),(1,1),(0,1)],o3)
for t in np.linspace(-1.3,1.3,4): q=c+dd*t+out*0.04; b_box('metal',(q[0],gm+2.9,q[1]),(0.06,ytm-gm-3.2,0.06),0)
for t in (-Lm/2+1.8,-Lm/2+4.6,Lm/2-4.6,Lm/2-1.8):
    if abs(t)<2.2: continue
    q=c+dd*t; yg=Hc(*(q+out*1.5)); q0=q-dd*1.2; q1=q+dd*1.2
    b_quad('door_maroon',(q0[0],yg,q0[1]),(q1[0],yg,q1[1]),(q1[0],yg+2.1,q1[1]),(q0[0],yg+2.1,q0[1]),[(0,0),(1,0),(1,1),(0,1)],o3)
rnd_=random.Random(7)
for x in np.arange(327.0,347.0,2.6):
    q=(x+rnd_.uniform(-.3,.3),212.0+rnd_.uniform(-.3,.3)); s_=rnd_.uniform(0.6,0.9); b_box('rock',(q[0],Hc(*q)-0.15,q[1]),(s_*1.2,s_*0.7,s_),rnd_.uniform(0,3))
lamp(np.array([348.5,211.5]),np.array([0.,-1.]))
# E) stepped white block (OSM 62597331, rue du Val Vert no 5): nDSM ~17-19 m wings, 20-23 m middle -> raised middle volume, mono-pitch roofs
pb,gb5,yb5,_,_=BI[62597331]; yt5=YT[62597331]
mid=np.array([[370.7,71.3],[378.8,71.4],[378.9,82.5],[370.7,82.5]]); ytm5=gb5+0.25+21.6-0.8
wall_bays(mid,yt5,ytm5,'apt',5,random.Random(5))
def mono(poly,y0,rise,dirv,mat='render_white'):   # mono-pitch slab rising along dirv, white gables
    poly=np.array(poly); t=poly@dirv; t0,t1=t.min(),t.max(); Y=[y0+rise*(v-t0)/(t1-t0) for v in t]
    P3=[(q[0],y,q[1]) for q,y in zip(poly,Y)]
    b_quad(mat,P3[0],P3[1],P3[2],P3[3],[(0,0),(1,0),(1,1),(0,1)],np.array([0,1.,0]))
    for i in range(4):
        a_,b_=P3[i],P3[(i+1)%4]
        if abs(a_[1]-b_[1])>0.05:
            lo=(a_ if a_[1]<b_[1] else b_); hi=(b_ if lo is a_ else a_)
            nn=np.cross(np.array(hi)-np.array(lo),[0,1,0]); cc=np.array(poly).mean(0); mm=(np.array([lo[0],lo[2]])+np.array([hi[0],hi[2]]))/2
            if np.dot([nn[0],nn[2]],mm-cc)<0: nn=-nn
            b_tri(mat,lo,hi,(hi[0],lo[1],hi[2]),[(0,0),(1,1),(1,0)],nn)
mono(mid,ytm5,2.2,np.array([1.,0]))
mono([[357.2,74.3],[370.7,74.4],[370.7,85.6],[357.1,85.6]],yt5,1.6,np.array([1.,0]))
mono([[364.4,85.6],[376.3,85.8],[376.3,92.7],[364.4,92.8]],yt5,1.4,np.array([0.,-1]))
# ping-pong table, paved footpath, black lamp post on the lawn between no 5 and #168
pc=np.array([371.5,104.5]); yg=Hc(*pc); b_box('concrete_board',(pc[0],yg+0.7,pc[1]),(2.74,0.08,1.525),0.05)
for s1 in(-0.8,0.8): b_box('concrete_board',(pc[0]+s1,yg-0.1,pc[1]+s1*0.04),(0.25,0.8,1.2),0.05)
b_box('metal',(pc[0],yg+0.78,pc[1]),(0.06,0.15,1.6),0.05)
pth=[(352.0,100.0),(362.0,101.5),(372.0,100.5),(382.0,102.0),(392.0,106.0)]; PL=[];PR=[]
for i,q in enumerate(pth):
    q=np.array(q); a_=np.array(pth[max(0,i-1)]); b_=np.array(pth[min(len(pth)-1,i+1)]); t=(b_-a_)/np.linalg.norm(b_-a_); nn=np.array([-t[1],t[0]])
    PL.append(tuple(q+nn*0.8)); PR.append(tuple(q-nn*0.8))
for i in range(len(pth)-1):
    sp=[PL[i],PL[i+1],PR[i+1],PR[i]]; drape(sp,'path_concrete',0.08,lambda x,z:(x/2.0,z/2.0),0.4); HIDE.extend([v for q in sp for v in q]); HIDEL.append(4)
lq=np.array([378.5,99.2]); yl=Hc(*lq); b_box('lamp_black',(lq[0],yl-0.2,lq[1]),(0.1,3.8,0.1),0); b_box('lamp_black',(lq[0],yl+3.6,lq[1]),(0.32,0.3,0.32),0); b_box('lamp_lens',(lq[0],yl+3.55,lq[1]),(0.26,0.06,0.26),0)
for m_,(V_,T_,UV_) in M.items(): addmesh('Env_'+m_,m_,V_,T_,UV_)
M.clear(); M.update(_Ms)
print('env168',nenv)
