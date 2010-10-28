--
-- PostgreSQL database dump
--

-- Started on 2010-10-28 22:19:37 CEST

SET statement_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = off;
SET check_function_bodies = false;
SET client_min_messages = warning;
SET escape_string_warning = off;

SET search_path = public, pg_catalog;

SET default_tablespace = '';

SET default_with_oids = false;

--
-- TOC entry 1499 (class 1259 OID 16398)
-- Dependencies: 1779 3
-- Name: gameinfo; Type: TABLE; Schema: public; Owner: postgres; Tablespace: 
--

CREATE TABLE gameinfo (
    id integer NOT NULL,
    "time" timestamp without time zone DEFAULT now() NOT NULL,
    map character varying NOT NULL,
    numplayers integer NOT NULL,
    maxplayers integer NOT NULL,
    gameserver_id integer NOT NULL
);


ALTER TABLE public.gameinfo OWNER TO postgres;

--
-- TOC entry 1498 (class 1259 OID 16396)
-- Dependencies: 3 1499
-- Name: gameinfo_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE gameinfo_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MAXVALUE
    NO MINVALUE
    CACHE 1;


ALTER TABLE public.gameinfo_id_seq OWNER TO postgres;

--
-- TOC entry 1794 (class 0 OID 0)
-- Dependencies: 1498
-- Name: gameinfo_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE gameinfo_id_seq OWNED BY gameinfo.id;


--
-- TOC entry 1795 (class 0 OID 0)
-- Dependencies: 1498
-- Name: gameinfo_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('gameinfo_id_seq', 32, true);


--
-- TOC entry 1497 (class 1259 OID 16387)
-- Dependencies: 3
-- Name: gameserver; Type: TABLE; Schema: public; Owner: postgres; Tablespace: 
--

CREATE TABLE gameserver (
    id integer NOT NULL,
    name character varying NOT NULL,
    host character varying NOT NULL,
    port integer NOT NULL
);


ALTER TABLE public.gameserver OWNER TO postgres;

--
-- TOC entry 1496 (class 1259 OID 16385)
-- Dependencies: 1497 3
-- Name: gameserver_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE gameserver_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MAXVALUE
    NO MINVALUE
    CACHE 1;


ALTER TABLE public.gameserver_id_seq OWNER TO postgres;

--
-- TOC entry 1796 (class 0 OID 0)
-- Dependencies: 1496
-- Name: gameserver_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE gameserver_id_seq OWNED BY gameserver.id;


--
-- TOC entry 1797 (class 0 OID 0)
-- Dependencies: 1496
-- Name: gameserver_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('gameserver_id_seq', 2, true);


--
-- TOC entry 1778 (class 2604 OID 16401)
-- Dependencies: 1498 1499 1499
-- Name: id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE gameinfo ALTER COLUMN id SET DEFAULT nextval('gameinfo_id_seq'::regclass);


--
-- TOC entry 1777 (class 2604 OID 16390)
-- Dependencies: 1497 1496 1497
-- Name: id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE gameserver ALTER COLUMN id SET DEFAULT nextval('gameserver_id_seq'::regclass);


--
-- TOC entry 1788 (class 0 OID 16398)
-- Dependencies: 1499
-- Data for Name: gameinfo; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY gameinfo (id, "time", map, numplayers, maxplayers, gameserver_id) FROM stdin;
9	2010-10-28 22:11:12.871199	TestSector	0	16	1
10	2010-10-28 22:11:12.904246	TestSector	0	16	2
11	2010-10-28 22:15:35.198026	TestSector	0	16	1
12	2010-10-28 22:15:35.220319	TestSector	0	16	2
13	2010-10-28 22:15:36.232308	TestSector	0	16	1
14	2010-10-28 22:15:36.245378	TestSector	0	16	2
15	2010-10-28 22:15:37.256124	TestSector	0	16	1
16	2010-10-28 22:15:37.270955	TestSector	0	16	2
17	2010-10-28 22:15:38.283291	TestSector	0	16	1
18	2010-10-28 22:15:38.297873	TestSector	0	16	2
19	2010-10-28 22:15:39.308745	TestSector	0	16	1
20	2010-10-28 22:15:39.324537	TestSector	0	16	2
21	2010-10-28 22:15:40.339618	TestSector	0	16	2
22	2010-10-28 22:15:40.349718	TestSector	0	16	1
23	2010-10-28 22:15:41.362013	TestSector	0	16	1
24	2010-10-28 22:15:41.376096	TestSector	0	16	2
25	2010-10-28 22:15:42.38767	TestSector	0	16	1
26	2010-10-28 22:15:42.413275	TestSector	0	16	2
27	2010-10-28 22:15:43.436567	TestSector	0	16	1
28	2010-10-28 22:15:43.465139	TestSector	0	16	2
29	2010-10-28 22:15:44.474179	TestSector	0	16	1
30	2010-10-28 22:15:44.493122	TestSector	0	16	2
31	2010-10-28 22:15:46.501527	TestSector	0	16	1
32	2010-10-28 22:15:46.51885	TestSector	0	16	2
\.


--
-- TOC entry 1787 (class 0 OID 16387)
-- Dependencies: 1497
-- Data for Name: gameserver; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY gameserver (id, name, host, port) FROM stdin;
1	Spacewar Arena #1	88.198.182.225	31400
2	Spacewar Arena #2	88.198.182.225	31402
\.


--
-- TOC entry 1785 (class 2606 OID 16407)
-- Dependencies: 1499 1499
-- Name: gameinfo_primary; Type: CONSTRAINT; Schema: public; Owner: postgres; Tablespace: 
--

ALTER TABLE ONLY gameinfo
    ADD CONSTRAINT gameinfo_primary PRIMARY KEY (id);


--
-- TOC entry 1781 (class 2606 OID 16418)
-- Dependencies: 1497 1497 1497
-- Name: gameserver_host_port_unique; Type: CONSTRAINT; Schema: public; Owner: postgres; Tablespace: 
--

ALTER TABLE ONLY gameserver
    ADD CONSTRAINT gameserver_host_port_unique UNIQUE (host, port);


--
-- TOC entry 1783 (class 2606 OID 16395)
-- Dependencies: 1497 1497
-- Name: gameserver_primary; Type: CONSTRAINT; Schema: public; Owner: postgres; Tablespace: 
--

ALTER TABLE ONLY gameserver
    ADD CONSTRAINT gameserver_primary PRIMARY KEY (id);


--
-- TOC entry 1786 (class 2606 OID 16408)
-- Dependencies: 1782 1499 1497
-- Name: gameinfo_gameserver_id; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY gameinfo
    ADD CONSTRAINT gameinfo_gameserver_id FOREIGN KEY (gameserver_id) REFERENCES gameserver(id) ON UPDATE CASCADE ON DELETE CASCADE;


--
-- TOC entry 1793 (class 0 OID 0)
-- Dependencies: 3
-- Name: public; Type: ACL; Schema: -; Owner: postgres
--

REVOKE ALL ON SCHEMA public FROM PUBLIC;
REVOKE ALL ON SCHEMA public FROM postgres;
GRANT ALL ON SCHEMA public TO postgres;
GRANT ALL ON SCHEMA public TO PUBLIC;


-- Completed on 2010-10-28 22:19:37 CEST

--
-- PostgreSQL database dump complete
--

