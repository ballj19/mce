;*****************************************************************************************
;											;*
;       Record Status for KCE								;*
; 											;*
; THE FOLLOWING TABLE SHOWS THE VALUE STORED IN THE STATUS BYTE 			;*
; AND THE ROUTINES TO BE CALLED TO RECORD THAT INFORMATION.     			;*
;                                                               			;*
;Value                    Service, Enable KCE, Key switch, KCE command			;*
;                                   feat. Sw						;*
; 00   SV0KF1KY0KC0     -  off      yes         off         off				;*
; 01   SV1KF1KY0KC1     -  on       yes         off         on				;*
; 02   SV1KF1KY1KC0     -  on       yes         on          off				;*
; 03   SV0KF0KY0KC0     -  off      no          off         off				;*
; 04   SV1KF1KY1KC0     -  on       no          on          off				;*
; 05   SVCNA            -  Service Not available					;*
; Enter:        R2, Feature number							;*
;*****************************************************************************************
ANSI_MAINLOOP:	MOV	R0,#PA_OPT_BYTE							;*
		MOV	A,@R0								;*
		SETB	PA_OPT		;ALWAYS SET FOR PCAODTHP VERSIONS 6.00.0000+	;*
		CLR	ANSI2K_PA							;*
		MOV	@R0,A								;*
		JB	SC_FLAG,FOUND_MAX_END						;*
											;*
;SOFTWARE ADDED FOR 06.02.0001-P.							;*
; 04-06-04, M.S.									;*
; OUT OF RANGE CHANGE.									;*
		CALL	CHK_FOR_ANSI2K		;CHECK THE ANSI 2000 OPTION.		;*
		JC	HDIO_MAINLOOP							;*
FOUND_MAX_END:	JMP	SKIP_HDIO_MAINLOOP	;IF NOT ANSI 2000 JOB THEN SKIP.	;*
											;*
HDIO_MAINLOOP:										;*
;  END 06.02.0001.									;*
											;*
		MOV	R0,#PA_OPT_BYTE							;*
		MOV	A,@R0								;*
		SETB	ANSI2K_PA							;*
		MOV	@R0,A								;*
											;*
		MOV	DPTR,#SW_STAGE1		;FIRST SOFTWARE STAGE FLAG.		;*
		MOVX	A,@DPTR								;*
		SETB	ACC.0								;*
		MOVX	@DPTR,A								;*
											;*
                jmp     ANSI_MAINLOOP
                JMP	Testing_JMP
		MOV	DPTR,#ANSI2KBYP							;*
		MOVX	A,@DPTR								;*
		MOV	C,ABYP			;IF BYPASS IS ON THEN SKIP		;*
		MOV	DPTR,#LTABYP_BYTE	;  THE REDUNDANCY CHECKS AND		;*
		MOVX	A,@DPTR			;  END OF RUN CYCLE TESTING.		;*
Testing_JMP:	ORL	C,LTABYP							;*
		JNC	DO_RED_CHECKS							;*
											;*
		MOV	DPTR,#FLT_ESTABLISHED	;CLEAR ALL EVENTS WHEN BYPASS IS	;*
		MOV	R0,#16			;  ACTIVE.				;*
		CALL	CLEAR_XRAM							;*
		MOV	DPTR,#FLT_EVENT_TBL						;*
		MOV	R0,#16								;*
		CALL	CLEAR_XRAM							;*
		MOV	DPTR,#FLT_ESTAB_MEM						;*
		MOV	R0,#16		   						;*
		CALL	CLEAR_XRAM	   						;*
		MOV	DPTR,#TMP_HARMS_ERR						;*
		MOV	R0,#6		   						;*
		CALL	CLEAR_XRAM	   						;*
											;*
;SOFTWARE ADDED FOR 06.02.0001-P.							;*
; 04-06-04, M.S.									;*
		JNB	SC_FLAG2,FOUND_MAX2						;*
		MOV	DPTR,#MPSAF_COUNTER						;*
		CLR	A								;*
		MOVX	@DPTR,A								;*
											;*
		MOV	DPTR,#RLULD_CNTR						;*
FOUND_MAX2:	MOVX	@DPTR,A								;*
											;*
;SOFTWARE ADDED FOR 06.03.0000-L.							;*
;  06-11-04, M.S.									;*
		MOV	DPTR,#EORCT							;*
		MOVX	@DPTR,A			;CLEAR THE TEST COMPLETE INDICATORS.	;*
											;*
		MOV	DPTR,#CT_BYTE		;UPON EXITING FROM ANSI BYPASS MODE	;*
		MOVX	A,@DPTR			;  THE END OF RUN CYCLE TEST MUST BE	;*
		SETB	RETEST_EOR		;  PERFORMED. SETTING THE RETEST_EOR  	;*
		MOVX	@DPTR,A			;  FLAG WILL CAUSE A CYCLE TEST.	;*
;  END 06.03.0000.									;*
											;*
		MOV	DPTR,#DZSTUCKOFF						;*
		MOVX	A,@DPTR								;*
		CLR	DZ_FAIL_OFF							;*
		MOVX	@DPTR,A								;*
;  END 06.02.0001.									;*
											;*
		MOV	DPTR,#RFR_BYTE							;*
		MOVX	A,@DPTR			    					;*
		CLR	RFRF			;CLEAR THE RFRF FLAG			;*
		MOVX	@DPTR,A								;*
		MOV	DPTR,#HDIO_RF							;*
		MOVX	A,@DPTR								;*
		MOV	C,RFR								;*
		ANL	C,/RFRM								;*
		JNC	SKIP_HDIO_MAINLOOP						;*
		MOV	DPTR,#RFR_BYTE							;*
		MOVX	A,@DPTR			    					;*
		SETB	RFRF			;SET THE RFRF FLAG			;*
		MOVX	@DPTR,A								;*
											;*
		MOV	DPTR,#PUMP_BYTE							;*
		MOVX	A,@DPTR								;*
		CLR	SS_UTSF								;*
		CLR	SS_FAIL								;*
		CLR	ABORT_PUMP							;*
		MOVX	@DPTR,A								;*
											;*
		CLR	A								;*
		MOV	DPTR,#PUMP_STAT							;*
		MOVX	@DPTR,A								;*
		INC	DPTR			;DPTR = PUMP_STAT_LATCH.		;*
		MOVX	@DPTR,A								;*
											;*
;SOFTWARE ADDED FOR 06.01.0000-M.							;*
;  10-14-02, M.S.									;*
		MOV	DPTR,#ANSI2K							;*
		MOVX	A,@DPTR								;*
		CLR	ETSL								;*
		MOVX	@DPTR,A								;*
;  END 06.01.0000.									;*
											;*
		SJMP	SKIP_HDIO_MAINLOOP						;*
											;*
DO_RED_CHECKS:	CALL	RED_CHECKS							;*
SKIP_CHECKS:	CALL	CHK_FLT_STATUS							;*
		CALL	EOR_CT								;*
											;*
;SOFTWARE ADDED FOR 06.03.0000-N.							;*
;  06-11-04, M.S.									;*
		CALL	GEN_MPSAF		;MPSAF OUTPUT LOGIC.		  	;*
;  END 06.03.0000.									;*

                CJNE    A,#03H,END_PUMP		;ONLY DO THIS FOR WYE-DELTA STARTERS.	;*
		JB	WYE1,END_PUMP		;WAIT FOR WYE OUTPUT TO TURN OFF BEFORE	;*
						;  BEGINNING THE DEL CONTACTOR		;*
						;  PROTECTION.				;*
		MOV	A,R2			;RESTORE CUR_PUMP TO A.			;*
		ADD	A,#DELPROT_TMR		;RESET CURRENT DEL PROTECTION TIMER.	;*
		MOV	TMRCOD,A		;DEL CONTACTOR PROTECTION.		;*
                				;  EXPIRED, THEN ABORT THE STARTER.	;*
		SJMP	END_PUMP

                											;*
SKIP_HDIO_MAINLOOP:									;*
											;*
		RET									;*

SV0KF1KY0KC0:	MOV     B,#00								;*
		SJMP    SC_RECORD							;*
											;*
SV1KF1KY0KC1:	MOV     B,#01								;*
		SJMP    SC_RECORD							;*
											;*
SV1KF1KY1KC0:	MOV     B,#02								;*
		SJMP    SC_RECORD							;*
											;*
SV0KF0KY0KC0:	MOV     B,#03								;*
		SJMP    SC_RECORD							;*
											;*
SV1KF0KY1KC0:	MOV     B,#04								;*
		SJMP    SC_RECORD							;*
											;*
SVCNA:		MOV     B,#05								;*
;		SJMP    SC_RECORD	;UNCOMMENT IF MORE OPTIONS ARE ADDED.		;*
											;*
SC_RECORD:	CALL    ADDFORFEATURE	;POINTING TO BYTE 1				;*
		INC     DPTR		;POINTING TO LANDING BYTE (#2)			;*
		INC     DPTR		;POINTING TO BYTE 3				;*
		INC     DPTR		;POINTING TO STATUS BYTE			;*
											;*
; NOW POINTING TO THE STATUS BYTE.							;*
END_PUMP:	MOV     A,B								;*
 		INC     DPTR		;POINTING TO BYTE 3				;*
		INC     DPTR		;POINTING TO STATUS BYTE			;*
		MOVX    @DPTR,A								;*
               	RET									;*
;  END KCE STATUS UPDATE.								;*
;*****************************************************************************************