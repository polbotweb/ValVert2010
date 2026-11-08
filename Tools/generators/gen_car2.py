# Val Vert: procedural 2000s French cars v2 (own work, CC0). Outputs OBJ LOD0/LOD1 with UVs + material groups.
import numpy as np,math
OUT='/workspace/cars/v2/'
class M:
    def __init__(s): s.V=[];s.T=[(0,0)];s.F={}
    def v(s,p): s.V.append(tuple(p)); return len(s.V)
    def t(s,u): s.T.append(tuple(u)); return len(s.T)
    def f(s,m,vs,ts=None): s.F.setdefault(m,[]).append((vs,ts or [1]*len(vs)))
    def quad_uv(s,m,vs):
        ts=[s.t(u) for u in((0,0),(1,0),(1,1),(0,1))]; s.f(m,vs,ts)
    def box(s,m,c,sz,uvface=None):
        b=[s.v((c[0]+dx*sz[0]/2,c[1]+dy*sz[1]/2,c[2]+dz*sz[2]/2)) for dx in(-1,1) for dy in(-1,1) for dz in(-1,1)]
        Q=[(0,1,3,2),(4,6,7,5),(0,4,5,1),(2,3,7,6),(0,2,6,4),(1,5,7,3)]
        for i,q in enumerate(Q):
            vs=[b[j] for j in q]
            if uvface is not None and i==uvface: s.quad_uv(m,vs)
            else: s.f(m,vs)
    def lathe(s,m,c,prof,n,axis=2,flip=False):
        # prof: list of (r, offset along axis); axis z (car width)
        R=[]
        for r,o in prof:
            ring=[]
            for j in range(n):
                a=2*math.pi*j/n; ring.append(s.v((c[0]+r*math.cos(a),c[1]+r*math.sin(a),c[2]+o)))
            R.append(ring)
        for i in range(len(R)-1):
            for j in range(n):
                q=[R[i][j],R[i][(j+1)%n],R[i+1][(j+1)%n],R[i+1][j]]
                s.f(m,q[::-1] if flip else q)
        return R
    def write(s,name):
        with open(OUT+name+'.obj','w') as o:
            o.write(f'mtllib {name}.mtl\n')
            for p in s.V: o.write('v %.4f %.4f %.4f\n'%(-p[0],p[1],p[2]))
            for u in s.T: o.write('vt %.4f %.4f\n'%u)
            for m,fs in s.F.items():
                o.write(f'g {m}\nusemtl {m}\n')
                for vs,ts in fs: o.write('f '+' '.join(f'{a}/{b}' for a,b in zip(vs,ts))+'\n')
        with open(OUT+name+'.mtl','w') as o:
            for m in s.F: o.write(f'newmtl {m}\nKd 0.5 0.5 0.5\n\n')
        return len(s.V)

def smooth(x,xs,ys): return float(np.interp(x,xs,ys))

def build(name,P,lod=0):
    m=M(); L,W=P['L'],P['W']; wb,fo=P['wb'],P['fo']; rw=P['rw']
    xw=[-L/2+fo,-L/2+fo+wb]
    NS=72 if lod==0 else 22; NR=12 if lod==0 else 5; NU=12 if lod==0 else 5
    ts=np.linspace(0,1,NS)
    rings=[]
    for t in ts:
        x=-L/2+t*L
        top=smooth(t,*zip(*P['roof']))
        belt=smooth(t,*zip(*P['belt']))
        belt=min(belt,top-0.01)
        hb=0.26
        for xc in xw:                                     # wheel arches
            d=abs(x-xc); ra=rw+0.06
            if d<ra: hb=max(hb,0.30+math.sqrt(ra*ra-d*d)*0.98)
        hb=min(hb,belt-0.08)
        hb=max(hb,smooth(t,[0,0.03,0.97,1],[0.42,0.28,0.28,0.44]))
        w=W/2*smooth(t,[0,0.015,0.05,0.12,0.88,0.95,0.985,1],[0.62,0.86,0.95,0.995,0.995,0.96,0.88,0.70])
        ring=[]
        for k in range(NR+1):
            a=k/NR*math.pi/2; c,s=math.cos(a),math.sin(a)
            z=w*abs(s)**0.30; y=hb+(belt-hb)*(1-abs(c)**0.30)
            # body-side tumble: slight bulge at mid height
            ring.append([x,y,z,'paint'])
        gt=P['glass'](t)
        for k in range(1,NU+1):
            u=k/NU
            ru=math.sin(u*math.pi/2)
            y=belt+(top-belt)*ru**0.55
            z=w*(1-P['tumble']*u**1.4)*math.cos(u*math.pi/2)**0.42
            if k==NU: z=0
            mat='paint'
            if gt=='side' and u<0.78: mat='glass'
            if gt=='front' and 0.08<u: mat='glass'
            if gt=='rear' and 0.18<u<0.97: mat='glass'
            if gt=='side' and any(abs(t-pb)<0.011 for pb in P['pillars']): mat='trim' if u<0.78 else mat
            ring.append([x,y,z,mat])
        rings.append(ring)
    R=len(rings[0])
    A=[[m.v(p[:3]) for p in r] for r in rings]
    B=[[m.v((p[0],p[1],-p[2])) for p in r] for r in rings]
    for i in range(NS-1):
        for k in range(R-1):
            ma=rings[i][k+1][3]; mb=rings[i+1][k+1][3]
            mat=ma if ma==mb else 'trim'
            if ma!=rings[i][k][3] and 'glass' in (ma,rings[i][k][3]) and lod==0: pass
            if rings[i][k][3]=='paint' and k>0 and ma=='glass' and mb=='glass' and rings[i][k][3]!=ma and lod==0 and False: pass
            m.f(mat,[A[i][k],A[i][k+1],A[i+1][k+1],A[i+1][k]])
            m.f(mat,[B[i][k],B[i+1][k],B[i+1][k+1],B[i][k+1]])
    for i,fl in((0,0),(NS-1,1)):
        c=m.v((rings[i][0][0],(rings[i][0][1]+rings[i][-1][1])/2,0))
        for k in range(R-1):
            a=[c,A[i][k+1],A[i][k]]; b=[c,B[i][k],B[i][k+1]]
            if fl: a=a[::-1]; b=b[::-1]
            m.f('paint',a); m.f('paint',b)
    # floor pan
    m.box('under',(0,0.29,0),(L*0.9,0.04,W*0.86))
    def side_z(t,y):
        # approx body surface z at station t and height y (for seams/lights)
        i=int(round(t*(NS-1))); r=rings[i]
        best=min(r,key=lambda p:abs(p[1]-y)); return best[2]
    # wheels
    for xc in xw:
        for s in(-1,1):
            zc=s*(W/2-0.13)
            if lod==0:
                tw=0.19; r0=rw; rr=rw*0.60
                prof=[(rr,-tw/2),(r0-0.035,-tw/2-0.008),(r0-0.008,-tw/2+0.012),(r0,-tw/4),(r0,tw/4),(r0-0.008,tw/2-0.012),(r0-0.035,tw/2+0.008),(rr,tw/2)]
                m.lathe('tyre',(xc,0.30,zc),prof,32)
                ro=s*(tw/2+0.002)
                m.lathe('rim',(xc,0.30,zc),[(rr,ro*0.98),(rr*0.97,ro*0.70),(rr*0.80,ro*0.55),(0.05,ro*0.50)],24,flip=(s<0))
                m.lathe('under',(xc,0.30,zc),[(rr*1.0,ro*0.99),(rr*0.96,ro*0.72)],24,flip=(s>0))
                if P['rim']=='alloy':
                    for j in range(5):
                        a=2*math.pi*j/5; cx=xc+math.cos(a)*rr*0.5; cy=0.30+math.sin(a)*rr*0.5
                        m.box('rim',(cx,cy,zc+ro*0.9),(0.05 if abs(math.cos(a))<0.5 else rr*0.8*abs(math.cos(a))+0.05,0.05 if abs(math.sin(a))<0.5 else rr*0.8*abs(math.sin(a))+0.05,0.025))
                else:
                    m.lathe('hubcap',(xc,0.30,zc),[(rr*0.88,ro*0.72),(rr*0.55,ro*0.92),(0.06,ro*1.0)],24,flip=(s<0))
                    for j in range(8):
                        a=2*math.pi*j/8; m.box('under',(xc+math.cos(a)*rr*0.72,0.30+math.sin(a)*rr*0.72,zc+ro*0.86),(0.035,0.035,0.01))
                m.box('trim',(xc,0.31,zc),(0.02,0.02,0.22))
            else:
                m.lathe('tyre',(xc,0.30,zc),[(rw*0.64,-0.095),(rw,-0.09),(rw,0.09),(rw*0.64,0.095)],10)
            m.lathe('under',(xc,0.30,s*(W/2-0.18)),[(rw+0.05,-0.15),(rw+0.05,0.15)],14 if lod==0 else 8,flip=True)
    if lod==1: return m
    xF=-L/2; xR=L/2
    # lights / grille / bumpers
    hy=P['hl_y']
    for s in(-1,1):
        zz=s*W*0.33; m.box('headlamp',(xF+P['hl_x'],hy,zz),(0.10,P['hl_h'],P['hl_w']))
        m.box('chrome',(xF+P['hl_x']+0.015,hy,zz),(0.09,P['hl_h']*0.55,P['hl_w']*0.5))
        m.box('taillamp',(xR-P['tl_x'],P['tl_y'],s*W*0.41),(0.08,P['tl_h'],0.14))
        m.box('indicator',(xR-P['tl_x']-0.004,P['tl_y']-P['tl_h']*0.3,s*W*0.41),(0.08,P['tl_h']*0.25,0.141))
        bt=smooth(P['mir'],*zip(*P['belt']))
        m.box('paint',(xF+P['mir']*L,bt+0.07,s*(W/2+0.07)),(0.10,0.11,0.15)); m.box('trim',(xF+P['mir']*L+0.03,bt+0.02,s*(W/2+0.02)),(0.05,0.05,0.06))
        # side rubbing strip + door seams + handles
        for i,ts_ in enumerate(np.linspace(0.18,0.82,24)):
            pass
        zs=side_z(0.5,0.62)+0.004
        m.box('trim',(0,0.62,s*zs),(L*0.62,0.05,0.012))
        for xs in P['seams']:
            ti=(xs+L/2)/L; zt=side_z(ti,0.6)+0.003
            bt=smooth(ti,*zip(*P['belt']))
            m.box('seam',(xs,(0.36+bt)/2,s*zt),(0.008,bt-0.38,0.006))
            m.box('trim',(xs-0.18,bt-0.08,s*(zt+0.004)),(0.12,0.025,0.01))
        # sill dirt
        m.box('dirt',(0,0.33,s*(side_z(0.5,0.34)+0.001)),(wb-0.75,0.07,0.01))
    m.box('trim',(xF+0.03,P['gr_y'],0),(0.06,P['gr_h'],W*0.42))
    ws0=P['ws']; tw_=ws0+0.01; xw_=-L/2+tw_*L; yw_=smooth(tw_,*zip(*P['roof']))+0.012
    for zz in(-0.28,0.32): m.box('trim',(xw_,yw_,zz),(0.03,0.012,0.55))
    m.box('chrome',(xF+0.01,P['gr_y'],0),(0.03,P['gr_h']*0.7,0.09))
    m.box('bumper',(xF+0.08,0.40,0),(0.16,0.16,W*0.95)); m.box('bumper',(xR-0.08,0.42,0),(0.16,0.17,W*0.95))
    m.box('plateF',(xF-0.005,0.42,0),(0.012,0.11,0.52),uvface=0)
    m.box('plateR',(xR+0.005,P['pl_y'],0),(0.012,0.11,0.52),uvface=1)
    m.box('trim',(0,0.33,0),(L*0.5,0.03,0.4))
    return m

def gl(ws,side,rw_):
    def g(t):
        if ws[0]<t<ws[1]: return 'front'
        if side[0]<=t<=side[1]: return 'side'
        if rw_[0]<t<rw_[1]: return 'rear'
        return None
    return g
B=dict(tumble=0.20,rw=0.29,rim='steel',hl_x=0.07,hl_h=0.13,hl_w=0.30,tl_x=0.05,tl_h=0.20,tl_y=0.82,gr_y=0.58,gr_h=0.08,pl_y=0.62,mir=0.33)
CARS={
 'clio2':dict(B,ws=0.265,L=3.81,W=1.64,wb=2.47,fo=0.70,hl_y=0.70,roof=[(0,0.58),(0.05,0.76),(0.25,0.90),(0.40,1.40),(0.50,1.42),(0.82,1.39),(0.93,1.28),(0.975,0.98),(1,0.80)],belt=[(0,0.70),(0.1,0.86),(0.4,0.92),(0.9,0.97),(1,0.92)],glass=gl((0.265,0.395),(0.405,0.915),(0.925,0.975)),pillars=[0.6],seams=[-0.45,0.62],mir=0.36,rim='steel'),
 'p206':dict(B,ws=0.29,L=3.83,W=1.65,wb=2.44,fo=0.74,hl_y=0.70,hl_w=0.36,roof=[(0,0.56),(0.05,0.74),(0.28,0.92),(0.43,1.40),(0.52,1.43),(0.78,1.40),(0.91,1.30),(0.97,1.00),(1,0.80)],belt=[(0,0.68),(0.1,0.84),(0.42,0.91),(0.9,1.0),(1,0.95)],glass=gl((0.29,0.425),(0.435,0.90),(0.915,0.965)),pillars=[0.61],seams=[-0.40,0.66],rim='alloy',tl_h=0.26),
 'megane2':dict(B,ws=0.245,L=4.21,W=1.78,wb=2.62,fo=0.82,hl_y=0.74,roof=[(0,0.62),(0.05,0.80),(0.23,0.96),(0.37,1.44),(0.47,1.47),(0.80,1.44),(0.88,1.36),(0.93,1.20),(0.97,1.10),(1,0.92)],belt=[(0,0.72),(0.1,0.90),(0.37,0.96),(0.9,1.03),(1,1.0)],glass=gl((0.245,0.365),(0.375,0.86),(0.875,0.93)),pillars=[0.6],seams=[-0.50,0.60],rim='alloy',tl_y=0.92,pl_y=0.70),
 'picasso':dict(B,ws=0.175,L=4.28,W=1.75,wb=2.76,fo=0.84,hl_y=0.76,roof=[(0,0.64),(0.05,0.84),(0.16,1.00),(0.30,1.60),(0.45,1.66),(0.88,1.63),(0.96,1.52),(0.99,1.10),(1,0.92)],belt=[(0,0.74),(0.1,0.92),(0.30,0.98),(0.9,1.02),(1,0.98)],glass=gl((0.175,0.295),(0.305,0.955),(0.958,0.985)),pillars=[0.55,0.8],seams=[-0.45,0.62],rim='steel',tl_y=1.0,tl_h=0.30,pl_y=0.72,tumble=0.12),
 'kangoo':dict(B,ws=0.21,L=3.99,W=1.67,wb=2.60,fo=0.72,hl_y=0.78,roof=[(0,0.66),(0.05,0.84),(0.20,0.98),(0.32,1.76),(0.45,1.80),(0.95,1.79),(0.99,1.6),(1,1.0)],belt=[(0,0.76),(0.1,0.94),(0.32,1.00),(1,1.02)],glass=gl((0.21,0.315),(0.325,0.97),(0.98,0.995)),pillars=[0.52,0.78],seams=[-0.42,0.45],rim='steel',tl_y=0.95,tl_h=0.32,pl_y=0.60,tumble=0.06),
 'twingo1':dict(B,ws=0.15,L=3.43,W=1.63,wb=2.35,fo=0.56,hl_y=0.74,hl_w=0.24,hl_h=0.16,roof=[(0,0.62),(0.05,0.80),(0.14,0.93),(0.30,1.40),(0.45,1.43),(0.85,1.40),(0.95,1.30),(0.99,1.0),(1,0.82)],belt=[(0,0.74),(0.1,0.90),(0.30,0.94),(1,0.98)],glass=gl((0.15,0.29),(0.30,0.94),(0.95,0.985)),pillars=[0.66],seams=[-0.30],rim='steel',tl_y=0.90,pl_y=0.65),
}
for n,P in CARS.items():
    for lod in(0,1):
        print(n,lod,build(n,P,lod).write(f'{n}_lod{lod}'))
